using System.Threading.Tasks;

namespace ChurchServices.Registration
{
    public interface IPhoneOtpSender
    {
        Task<bool> SendOtpAsync(string phoneNumber, string otp);
    }
}
