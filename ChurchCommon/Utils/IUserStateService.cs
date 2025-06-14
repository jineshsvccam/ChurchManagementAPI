using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchCommon.Utils
{
    public interface IUserStateService
    {
        Task SetStateAsync(string userMobile, string state, TimeSpan? expiry = null);
        Task<string?> GetStateAsync(string userMobile);
        Task ClearStateAsync(string userMobile);
    }
}
