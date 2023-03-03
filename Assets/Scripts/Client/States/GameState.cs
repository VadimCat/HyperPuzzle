using Cysharp.Threading.Tasks;
using Ji2Core.Core.States;

namespace Client.States
{
    public class GameState : IState
    {
        // private readonly LoadingPresenterFactory loadingPresenterFactory;
        //
        // public GameState(LoadingPresenterFactory loadingPresenterFactory)
        // {
        //     this.loadingPresenterFactory = loadingPresenterFactory;
        // }

        public async UniTask Enter()
        {
            // await loadingPresenterFactory.Create(5).LoadAsync();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}