using Client.UI.Screens;
using Client.Views.Level;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Models;

namespace Client.States
{
    public class LevelCompletedState : IPayloadedState<LevelCompletedState.Payload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly LevelsConfig levelsConfig;
        private readonly AudioService audioService;

        public LevelCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress, LevelsConfig levelsConfig, AudioService audioService)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.levelsLoopProgress = levelsLoopProgress;
            this.levelsConfig = levelsConfig;
            this.audioService = audioService;
        }

        public async UniTask Enter(Payload payload)
        {
            var screen = await screenNavigator.PushScreen<LevelCompletedScreen>();
            var levelName = payload.level.Name;
            var levelViewConfig = levelsConfig.GetData(levelName);
            
            screen.SetLevelResult(levelViewConfig.Image, payload.level.LevelCount);

            screen.ClickNextEvent += OnClickNext;
        }

        private void OnClickNext()
        {
            // audioService.PlaySfxAsync(AudioClipName.ButtonFX);
            var levelData = levelsLoopProgress.GetNextLevelData();
            stateMachine.Enter<LoadLevelState, LoadLevelState.Payload>(new LoadLevelState.Payload(levelData, 1f));
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LevelCompletedScreen>();
        }
        
        public class Payload
        {
            public Level level;
        }
    }
}