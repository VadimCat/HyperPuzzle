using Client.Presenters;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Models;

namespace Client.States
{
    public class GameState : IPayloadedState<GameState.Payload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private Level _model;

        public GameState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
        }

        public class Payload
        {
            public LevelPresenter levelPresenter;
        }

        public async UniTask Enter(Payload payload)
        {
            await screenNavigator.PushScreen<LevelScreen>();
            payload.levelPresenter.EventLevelCompleted += OnComplete;
            payload.levelPresenter.StartLevel();
            _model = payload.levelPresenter.Model;
        }

        private void OnComplete()
        {
            stateMachine.Enter<LevelCompletedState, LevelCompletedState.Payload>(new LevelCompletedState.Payload
                { level = _model });
        }

        public UniTask Exit()
        {
            return screenNavigator.CloseScreen<LevelScreen>();
        }
    }
}