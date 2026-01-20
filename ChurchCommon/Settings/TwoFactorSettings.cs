namespace ChurchCommon.Settings
{
    public class TwoFactorSettings
    {
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Maximum number of active 2FA sessions allowed per user within the rate limit window.
        /// </summary>
        public int MaxSessionsPerUser { get; set; } = 3;
        
        /// <summary>
        /// Time window in minutes for rate limiting 2FA session creation.
        /// </summary>
        public int RateLimitWindowMinutes { get; set; } = 10;
    }
}
