using System.Linq.Expressions;

namespace Core.ServicesAbstraction;

public interface IBackgroundService
{
    void ExecuteInBackground(Expression<Action> job);
}
