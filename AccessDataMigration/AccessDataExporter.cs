using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Text.Json;
using ChurchData;
using ChurchDTOs.DTOs.Entities;

public class AccessDataExporter
{
    public int ParishId { get; set; }
    public List<TransactionHeadDto> ExportTransactionHeads(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var transactionHeads = new List<TransactionHeadDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var transactionHead = new TransactionHeadDto
                    {
                        HeadName = reader["HeadName"].ToString(),
                        Type = reader["Typ"].ToString(),
                        IsMandatory = reader["Remarks"].ToString().Contains("Kudishika"),
                        Description = reader["Remarks"].ToString(),
                        Aramanapct = reader.IsDBNull(reader.GetOrdinal("Aramana")) || string.IsNullOrEmpty(reader["Aramana"].ToString()) ? 0 : Convert.ToDouble(reader["Aramana"]),
                        Ordr = reader["Odr"].ToString(),
                        HeadNameMl = "",
                        Action = "INSERT",
                        ParishId = ParishId
                    };
                    ////reader["HdNameL"].ToString()""
                    transactionHeads.Add(transactionHead);
                }
            }
        }

        return transactionHeads;
    }

    public List<UnitDto> ExportUnits(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var units = new List<UnitDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open(); // Use synchronous open method
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader()) // Use OleDbDataReader
            {
                while (reader.Read()) // Use synchronous read method
                {
                    var unit = new UnitDto
                    {
                        UnitName = reader["Unit"].ToString(),
                        Description = "",
                        UnitPresident = "",
                        UnitSecretary = "",
                        ParishId = ParishId
                    };

                    units.Add(unit);
                }
            }
        }
        return units;
    }

    public List<FamilyDto> ExportFamilies(string accessDbPath, string tableName, Dictionary<string, int> unitNameLookup)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var families = new List<FamilyDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var words = reader["MemName"].ToString().Split();
                    var headName = words.Length > 0 ? words[0] : "head";
                    var familyName = words.Length > 1 ? words[^1] : reader["MemName"].ToString();
                    var status = string.IsNullOrEmpty(reader["stat"].ToString()) ? "Live" : reader["stat"].ToString();

                    var unitName = reader["UnitName"].ToString();
                    var unitID = unitNameLookup.ContainsKey(unitName) ? unitNameLookup[unitName] : 0;

                    var family = new FamilyDto
                    {
                        Action = "INSERT",
                        UnitId = unitID,
                        ParishId = ParishId,
                        FamilyName = familyName,
                        Address = null, // Set to null or modify as necessary
                        ContactInfo = null, // Set to null or modify as necessary
                        Category = "Low", // Default category, modify as necessary
                        FamilyNumber = Convert.ToInt32(reader["Hno"]),
                        Status = status,
                        HeadName = headName
                    };

                    families.Add(family);
                }
            }
        }

        return families;
    }

    public List<BankDto> ExportBanks(string accessDbPath, string tableName)
    {
        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var banks = new List<BankDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var bank = new BankDto
                    {
                        BankName = reader["BankName"].ToString(),
                        AccountNumber = reader["AccountNo"].ToString(),
                        OpeningBalance = Convert.ToDecimal(reader["OB"]),
                        CurrentBalance = Convert.ToDecimal(reader["OB"]), // Assuming current balance is the same as opening balance for now
                        ParishId = ParishId,
                        Action = "INSERT"
                    };

                    banks.Add(bank);
                }
            }
        }

        return banks;
    }

    public List<FamilyDueDto> ExportFamilyDues(string accessDbPath, string tableName, Dictionary<string, int> headNameLookup, Dictionary<string, int> familyNameLookup)
    {

        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var familyDues = new List<FamilyDueDto>();


        var headNameLookupInsensitive = new Dictionary<string, int>(headNameLookup, StringComparer.OrdinalIgnoreCase);
        var familyNameLookupInsensitive = new Dictionary<string, int>(familyNameLookup, StringComparer.OrdinalIgnoreCase);


        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var due = new FamilyDueDto
                    {
                        HeadId = headNameLookupInsensitive.TryGetValue(reader["KudiHead"].ToString(), out int headId) ? headId : 0,
                        FamilyId = familyNameLookupInsensitive.TryGetValue(reader["Hno"].ToString(), out int familyId) ? familyId : 0,
                        ParishId = ParishId,
                        OpeningBalance = Convert.ToDecimal(reader["kudiamount"])
                    };

                    familyDues.Add(due);
                }
            }
        }

        return familyDues;
    }

    public List<ContributionSettingsDto> ExportContributionsSettings(string accessDbPath, string tableName, Dictionary<string, int> headNameLookup)
    {

        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var contributionSettings = new List<ContributionSettingsDto>();


        var headNameLookupInsensitive = new Dictionary<string, int>(headNameLookup, StringComparer.OrdinalIgnoreCase);



        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {tableName}", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var category = reader["cat"].ToString();
                    if (category == "A" || category == "B" || category == "C")
                    {
                        var due = new ContributionSettingsDto
                        {
                            HeadId = headNameLookupInsensitive.TryGetValue(reader["Items"].ToString(), out int headId) ? headId : 0,
                            ParishId = ParishId,
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            FineAmount = Convert.ToDecimal(reader["Fine"]),
                            Category = category == "A" ? "High" : category == "B" ? "Middle" : "Low",
                            Frequency = reader["Items"].ToString() == "Masavari" ? "Monthly" : "Annually",
                            ValidFrom = Convert.ToDateTime(reader["Duedate"])
                        };
                        contributionSettings.Add(due);
                    }


                }
            }
        }

        return contributionSettings;
    }


    public List<TransactionDto> ExportTransactions(string accessDbPath, string tableName, Dictionary<string, int> headNameLookup, Dictionary<string, int> familyNameLookup, Dictionary<string, int> bankNameLookup)
    {
        // Create case-insensitive dictionaries
        var headNameLookupInsensitive = new Dictionary<string, int>(headNameLookup, StringComparer.OrdinalIgnoreCase);
        var familyNameLookupInsensitive = new Dictionary<string, int>(familyNameLookup, StringComparer.OrdinalIgnoreCase);
        var bankNameLookupInsensitive = new Dictionary<string, int>(bankNameLookup, StringComparer.OrdinalIgnoreCase);

        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var transactions = new List<TransactionDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT {tableName}.* FROM {tableName} where CB<>\"JE\"", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var transactionType = reader["Type"].ToString();
                    transactionType = transactionType == "Receipt" ? "Income" : "Expense";

                    var transaction = new TransactionDto
                    {
                        Action = "INSERT",
                        TransactionId = Convert.ToInt32(reader["ID"]),
                        TrDate = Convert.ToDateTime(reader["VRDate"]),
                        VrNo = reader["VRNo"].ToString(),
                        TransactionType = transactionType,
                        HeadId = headNameLookupInsensitive.TryGetValue(reader["Head"].ToString(), out int headId) ? headId : 0,
                        FamilyId = familyNameLookupInsensitive.TryGetValue(reader["Hno"].ToString(), out int familyId) ? familyId : 0,
                        BankId = bankNameLookupInsensitive.TryGetValue(reader["CB"].ToString(), out int bankId) ? bankId : 0,
                        IncomeAmount = Convert.ToDecimal(reader["Credit"]),
                        ExpenseAmount = Convert.ToDecimal(reader["Debit"]),
                        Description = reader["Remarks"].ToString(),
                        ParishId = ParishId,
                        BillName = reader["MemName"].ToString()
                    };

                    transactions.Add(transaction);
                }
            }
        }

        return transactions;
    }

    public List<FamilyContributionDto> ExportTransactionsJE(string accessDbPath, string tableName, Dictionary<string, int> headNameLookup, Dictionary<string, int> familyNameLookup, Dictionary<string, int> bankNameLookup)
    {
        // Create case-insensitive dictionaries
        var headNameLookupInsensitive = new Dictionary<string, int>(headNameLookup, StringComparer.OrdinalIgnoreCase);
        var familyNameLookupInsensitive = new Dictionary<string, int>(familyNameLookup, StringComparer.OrdinalIgnoreCase);
        var bankNameLookupInsensitive = new Dictionary<string, int>(bankNameLookup, StringComparer.OrdinalIgnoreCase);

        string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
        var transactions = new List<FamilyContributionDto>();

        using (OleDbConnection conn = new OleDbConnection(connString))
        {
            conn.Open();
            using (OleDbCommand cmd = new OleDbCommand($"SELECT {tableName}.* FROM {tableName} where CB=\"JE\"", conn))
            using (OleDbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var transactionType = reader["Type"].ToString();
                    transactionType = transactionType == "Receipt" ? "Income" : "Expense";

                    var transaction = new FamilyContributionDto
                    {
                        ContributionId = Convert.ToInt32(reader["ID"]),
                        TransactionDate = Convert.ToDateTime(reader["VRDate"]),
                        VoucherNumber = reader["VRNo"].ToString(),
                        TransactionType = transactionType,
                        HeadId = headNameLookupInsensitive.TryGetValue(reader["Head"].ToString(), out int headId) ? headId : 0,
                        FamilyId = familyNameLookupInsensitive.TryGetValue(reader["Hno"].ToString(), out int familyId) ? familyId : 0,
                        BankId = bankNameLookupInsensitive.TryGetValue(reader["CB"].ToString(), out int bankId) ? bankId : 0,
                        IncomeAmount = Convert.ToDecimal(reader["Credit"]),
                        ExpenseAmount = Convert.ToDecimal(reader["Debit"]),
                        Description = reader["Remarks"].ToString(),
                        ParishId = ParishId,
                        BillName = reader["MemName"].ToString()
                    };

                    transactions.Add(transaction);
                }
            }
        }

        return transactions;
    }


    public List<PendingFamilyMemberRequestDto> ExportFamilyMembers(string accessDbPath, string tableName, Dictionary<string, int> unitNameLookup, Dictionary<string, int> familyNameLookup)
    {
        try
        {
            var familyNameLookupInsensitive = new Dictionary<string, int>(familyNameLookup, StringComparer.OrdinalIgnoreCase);
            var unitNameLookupInsensitive = new Dictionary<string, int>(unitNameLookup, StringComparer.OrdinalIgnoreCase);

            string connString = ConfigurationManager.ConnectionStrings["AccessConnectionString"].ConnectionString;
            var familyMembers = new List<PendingFamilyMemberRequestDto>();

            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();
                using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM [{tableName}]", conn))
                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int familyId = familyNameLookupInsensitive.TryGetValue(reader["Houseno"].ToString(), out int fid) ? fid : 0;
                        int? unitId = unitNameLookupInsensitive.TryGetValue(reader["Unit"].ToString(), out int uid) ? uid : 0;

                        var familyMember = new FamilyMemberDto
                        {
                            FamilyId = familyId,
                            ParishId=ParishId,
                            UnitId = unitId,
                            FamilyNumber = Convert.ToInt32(reader["Houseno"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Nickname = reader["ChildName"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            DateOfBirth = DateTime.TryParse(reader["Dob"].ToString(), out var dob) ? dob : null,
                            MaritalStatus = !string.IsNullOrEmpty(reader["MDate"].ToString()) ? "Married" : "Single",
                            ActiveMember = true,
                            MemberStatus = reader["Member"].ToString() == "Yes" ? "Alive" : "Left",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,

                            Contacts = new List<FamilyMemberContactsDto>
                        {
                            new FamilyMemberContactsDto
                            {
                                AddressLine2 = reader["Add2"].ToString(),
                                AddressLine3 = reader["Add3"].ToString(),
                                PostOffice = reader["PO"].ToString(),
                                PinCode = reader["Pin"].ToString(),
                                LandPhone = reader["Landphone"].ToString(),
                                MobilePhone = reader["Mobile"].ToString(),
                                Email = reader["Email"].ToString(),
                                FacebookProfile = reader["Facebook"].ToString(),
                                GeoLocation = ""
                            }
                        },
                            Identity = new FamilyMemberIdentityDto
                            {
                                AadharNumber = "",
                                PassportNumber = "",
                                DrivingLicense = "",
                                VoterId = ""
                            },
                            Occupation = new FamilyMemberOccupationDto
                            {
                                Qualification = reader["Quali"].ToString(),
                                StudentOrEmployee = "",
                                ClassOrWork = reader["CW"].ToString(),
                                SchoolOrWorkplace = "",
                                SundaySchoolClass = ""
                            },
                            Sacraments = new FamilyMemberSacramentsDto
                            {
                                BaptismalName = "",
                                BaptismDate = null,
                                MarriageDate = null,
                                MooronDate = null,
                                MooronInOurChurch = false,
                                MarriageInOurChurch = true,
                                BaptismInOurChurch = false
                            },
                            Relations = new List<FamilyMemberRelationsDto>
                        {
                            new FamilyMemberRelationsDto
                            {
                                FatherName = "",
                                MotherName = "",
                                SpouseId = 0,
                                ParentId = 0
                            }
                        },
                            Files = new FamilyMemberFilesDto
                            {
                                MarriageFileNo = "",
                                BaptismFileNo = "",
                                DeathFileNo = "",
                                JoinFileNo = "",
                                MooronFileNo = "",
                                CommonCellNo = ""
                            },
                            Lifecycle = new FamilyMemberLifecycleDto
                            {
                                CommonCell = false,
                                LeftReason = "",
                                JoinDate = null,
                                LeftDate = null,
                                BurialPlace = "",
                                DeathDate = null
                            }
                        };

                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        var json = JsonSerializer.SerializeToElement(familyMember, options);

                     //   var json = JsonSerializer.SerializeToElement(familyMember);

                        var pending = new PendingFamilyMemberRequestDto
                        {
                            FamilyId = familyId,
                            ParishId = ParishId,
                            ActionType="INSERT",                          
                            Payload = json                           
                        };

                        familyMembers.Add(pending);
                    }
                }
            }
           

            return familyMembers;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public List<List<T>> SplitList<T>(List<T> source, int batchSize)
    {
        var batches = new List<List<T>>();
        for (int i = 0; i < source.Count; i += batchSize)
        {
            batches.Add(source.GetRange(i, Math.Min(batchSize, source.Count - i)));
        }
        return batches;
    }
}
