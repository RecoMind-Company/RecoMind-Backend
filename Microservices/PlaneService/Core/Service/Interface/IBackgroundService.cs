using System.Linq.Expressions;

namespace Core.Service.Interface;

public interface IBackgroundService
{
    void ExecuteInBackground(Expression<Action> job);
}
