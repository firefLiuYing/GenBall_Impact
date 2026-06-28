using System.Threading.Tasks;
using Yueyn.Main;

namespace GenBall.Procedure
{
    public interface IUserSettingsStorage : ISystem
    {
        UserSettings Settings { get; }
        Task SaveAsync();
        void ApplyToRuntime();
    }
}
