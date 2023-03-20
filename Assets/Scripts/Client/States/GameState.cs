using Client.Presenters;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;

namespace Client.States
{
    public class GameState : IPayloadedState<GameState.Payload>
    {
        private readonly ScreenNavigator screenNavigator;

        public GameState(ScreenNavigator screenNavigator)
        {
            this.screenNavigator = screenNavigator;
        }
        public class Payload
        {
            public LevelPresenter levelPresenter;
        }

        public async UniTask Enter(Payload payload)
        {
            await screenNavigator.PushScreen<LevelScreen>();
            payload.levelPresenter.StartLevel();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}