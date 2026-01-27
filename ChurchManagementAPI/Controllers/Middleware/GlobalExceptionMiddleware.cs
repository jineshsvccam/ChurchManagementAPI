using System.Net;
using System.Text.Json;
using Npgsql;

namespace ChurchManagementAPI.Controllers.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string errorMessage = "An unexpected error occurred. Please try again later.";

            // Check for PostgreSQL exceptions (FK violations, unique constraints, etc.)
            var postgresException = GetPostgresException(exception);
            if (postgresException != null)
            {
                (statusCode, errorMessage) = HandlePostgresException(postgresException);
            }
            else
            {
                switch (exception)
                {
                    case KeyNotFoundException:
                        statusCode = HttpStatusCode.NotFound;
                        errorMessage = "Resource not found.";
                        break;

                    case UnauthorizedAccessException:
                        statusCode = HttpStatusCode.Unauthorized;
                        errorMessage = "Unauthorized access.";
                        break;

                    case ArgumentNullException:
                    case ArgumentException:
                        statusCode = HttpStatusCode.BadRequest;
                        errorMessage = exception.Message;
                        break;

                    case InvalidOperationException:
                        statusCode = HttpStatusCode.BadRequest;
                        errorMessage = exception.Message;
                        break;

                    case FormatException:
                        statusCode = HttpStatusCode.BadRequest;
                        errorMessage = "Invalid format.";
                        break;

                    case TimeoutException:
                        statusCode = HttpStatusCode.RequestTimeout;
                        errorMessage = "Request timed out.";
                        break;
                }
            }

            // Log full exception details including inner exceptions
            _logger.LogError(exception, "Exception Occurred: {ExceptionType} | Path: {RequestPath} | Message: {ErrorMessage}",
                exception.GetType().Name, context.Request.Path, exception.Message);

            LogInnerExceptions(exception);

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = errorMessage,
                Detailed = _env.IsDevelopment() ? exception.Message : null,
                StackTrace = _env.IsDevelopment() && exception.InnerException != null ? exception.InnerException.ToString() : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

        /// <summary>
        /// Extracts PostgresException from the exception chain if present.
        /// </summary>
        private PostgresException? GetPostgresException(Exception exception)
        {
            if (exception is PostgresException postgresEx)
                return postgresEx;

            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is PostgresException innerPostgresEx)
                    return innerPostgresEx;
                inner = inner.InnerException;
            }

            return null;
        }

        /// <summary>
        /// Handles PostgreSQL-specific exceptions and returns appropriate HTTP status and message.
        /// </summary>
        private (HttpStatusCode statusCode, string message) HandlePostgresException(PostgresException postgresException)
        {
            // PostgreSQL error codes: https://www.postgresql.org/docs/current/errcodes-appendix.html
            return postgresException.SqlState switch
            {
                // Foreign key violation
                "23503" => (HttpStatusCode.BadRequest, BuildForeignKeyErrorMessage(postgresException)),

                // Unique constraint violation
                "23505" => (HttpStatusCode.Conflict, BuildUniqueConstraintErrorMessage(postgresException)),

                // Not null violation
                "23502" => (HttpStatusCode.BadRequest, BuildNotNullErrorMessage(postgresException)),

                // Check constraint violation
                "23514" => (HttpStatusCode.BadRequest, BuildCheckConstraintErrorMessage(postgresException)),

                // String data right truncation
                "22001" => (HttpStatusCode.BadRequest, "Input data is too long for the field."),

                // Numeric value out of range
                "22003" => (HttpStatusCode.BadRequest, "Numeric value is out of the allowed range."),

                // Invalid text representation
                "22P02" => (HttpStatusCode.BadRequest, "Invalid data format provided."),

                // Default: return generic database error without exposing details
                _ => (HttpStatusCode.InternalServerError, "A database error occurred. Please try again later.")
            };
        }

        /// <summary>
        /// Builds a user-friendly message for foreign key violations.
        /// </summary>
        private string BuildForeignKeyErrorMessage(PostgresException ex)
        {
            var constraintName = ex.ConstraintName;
            var tableName = ex.TableName;
            var messageText = ex.MessageText ?? string.Empty;

            // Check if this is a DELETE/UPDATE restriction (record has dependencies)
            // vs an INSERT/UPDATE with invalid reference (referenced record doesn't exist)
            bool isDeleteRestriction = messageText.Contains("update or delete on table", StringComparison.OrdinalIgnoreCase);

            if (isDeleteRestriction)
            {
                // This is a DELETE operation being blocked because other records depend on this one
                return BuildDeleteRestrictionMessage(constraintName, tableName);
            }
            else
            {
                // This is an INSERT/UPDATE with an invalid foreign key reference
                return BuildInvalidReferenceMessage(constraintName);
            }
        }

        /// <summary>
        /// Builds error message when trying to delete a record that has dependent records.
        /// </summary>
        private string BuildDeleteRestrictionMessage(string? constraintName, string? tableName)
        {
            // Map constraint names to friendly messages for delete restrictions
            return constraintName switch
            {
                // TransactionHead dependencies
                "contribution_settings_head_id_fkey" => "Cannot delete this transaction head because it is referenced by contribution settings.",
                "transactions_head_id_fkey" => "Cannot delete this transaction head because it has associated transactions.",
                "family_dues_head_id_fkey" => "Cannot delete this transaction head because it is referenced by family dues.",

                // Family dependencies
                "family_members_family_id_fkey" => "Cannot delete this family because it has family members.",
                "transactions_family_id_fkey" => "Cannot delete this family because it has associated transactions.",
                "family_dues_family_id_fkey" => "Cannot delete this family because it has associated dues.",
                "family_files_family_id_fkey" => "Cannot delete this family because it has associated files.",

                // Unit dependencies
                "families_unit_id_fkey" => "Cannot delete this unit because it has associated families.",

                // Parish dependencies
                "units_parish_id_fkey" => "Cannot delete this parish because it has associated units.",
                "families_parish_id_fkey" => "Cannot delete this parish because it has associated families.",
                "transaction_heads_parish_id_fkey" => "Cannot delete this parish because it has associated transaction heads.",
                "banks_parish_id_fkey" => "Cannot delete this parish because it has associated banks.",
                "transactions_parish_id_fkey" => "Cannot delete this parish because it has associated transactions.",

                // Bank dependencies
                "transactions_bank_id_fkey" => "Cannot delete this bank because it has associated transactions.",

                // Generic patterns based on constraint name
                var c when c?.Contains("head_id") == true => "Cannot delete this transaction head because it is referenced by other records.",
                var c when c?.Contains("family_id") == true => "Cannot delete this family because it is referenced by other records.",
                var c when c?.Contains("unit_id") == true => "Cannot delete this unit because it is referenced by other records.",
                var c when c?.Contains("parish_id") == true => "Cannot delete this parish because it is referenced by other records.",
                var c when c?.Contains("bank_id") == true => "Cannot delete this bank because it is referenced by other records.",
                var c when c?.Contains("member_id") == true => "Cannot delete this family member because it is referenced by other records.",

                // Default message
                _ => $"Cannot delete this record because it is referenced by other records in '{tableName ?? "the database"}'."
            };
        }

        /// <summary>
        /// Builds error message when inserting/updating with an invalid foreign key reference.
        /// </summary>
        private string BuildInvalidReferenceMessage(string? constraintName)
        {
            // Map common constraint names to friendly messages for invalid references
            return constraintName switch
            {
                var c when c?.Contains("parish") == true => "Invalid ParishId: The specified parish does not exist.",
                var c when c?.Contains("family") == true => "Invalid FamilyId: The specified family does not exist.",
                var c when c?.Contains("unit") == true => "Invalid UnitId: The specified unit does not exist.",
                var c when c?.Contains("head") == true => "Invalid HeadId: The specified transaction head does not exist.",
                var c when c?.Contains("bank") == true => "Invalid BankId: The specified bank does not exist.",
                var c when c?.Contains("member") == true => "Invalid MemberId: The specified family member does not exist.",
                _ => "Invalid reference: The specified related record does not exist."
            };
        }

        /// <summary>
        /// Builds a user-friendly message for unique constraint violations.
        /// </summary>
        private string BuildUniqueConstraintErrorMessage(PostgresException ex)
        {
            var constraintName = ex.ConstraintName;

            return constraintName switch
            {
                var c when c?.Contains("email") == true => "A record with this email already exists.",
                var c when c?.Contains("name") == true => "A record with this name already exists.",
                var c when c?.Contains("number") == true => "A record with this number already exists.",
                _ => "A record with the same unique value already exists."
            };
        }

        /// <summary>
        /// Builds a user-friendly message for not-null constraint violations.
        /// </summary>
        private string BuildNotNullErrorMessage(PostgresException ex)
        {
            var columnName = ex.ColumnName;

            if (!string.IsNullOrEmpty(columnName))
            {
                // Convert snake_case to PascalCase for display
                var friendlyName = string.Join("", columnName.Split('_').Select(s =>
                    char.ToUpper(s[0]) + s.Substring(1)));
                return $"The field '{friendlyName}' is required and cannot be null.";
            }

            return "A required field is missing.";
        }

        /// <summary>
        /// Builds a user-friendly message for check constraint violations.
        /// </summary>
        private string BuildCheckConstraintErrorMessage(PostgresException ex)
        {
            var constraintName = ex.ConstraintName;
            var tableName = ex.TableName;

            // Map known check constraints to friendly messages
            return constraintName switch
            {
                // TransactionHead constraints
                "transaction_head_type_check" => "Invalid Type value. Allowed values are: 'Income', 'Expense', 'Both'.",

                // Transaction constraints
                "transaction_type_check" => "Invalid TransactionType value. Allowed values are: 'Income', 'Expense'.",
                "transaction_income_amount_check" => "Income amount must be greater than or equal to 0.",
                "transaction_expense_amount_check" => "Expense amount must be greater than or equal to 0.",

                // ContributionSettings constraints
                "contribution_settings_category_check" => "Invalid Category value. Allowed values are: 'Low', 'Middle', 'High'.",
                "contribution_settings_frequency_check" => "Invalid Frequency value. Allowed values are: 'Monthly', 'Annually'.",

                // Family constraints
                "family_category_check" => "Invalid Category value. Allowed values are: 'Low', 'Middle', 'High'.",
                "family_status_check" => "Invalid Status value. Allowed values are: 'Live', 'Left', 'Dead'.",

                // Generic patterns
                var c when c?.Contains("amount") == true => "Amount must be a non-negative value.",
                var c when c?.Contains("balance") == true => "Balance must be a non-negative value.",
                var c when c?.Contains("category") == true => "Invalid category value provided.",
                var c when c?.Contains("frequency") == true => "Invalid frequency value provided.",
                var c when c?.Contains("status") == true => "Invalid status value provided.",
                var c when c?.Contains("type") == true => $"Invalid type value provided for {tableName ?? "record"}.",
                _ => $"Data validation failed: The value provided violates a constraint on '{tableName ?? "the table"}'."
            };
        }

        private void LogInnerExceptions(Exception ex)
        {
            Exception? inner = ex.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Inner Exception: {ExceptionType} | Message: {ErrorMessage}", inner.GetType().Name, inner.Message);
                inner = inner.InnerException;
            }
        }
    }
}
