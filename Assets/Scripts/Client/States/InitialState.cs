using Cysharp.Threading.Tasks;
using Facebook.Unity;
using Ji2.CommonCore.SaveDataContainer;
using Ji2Core.Core;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using UI.Screens;
using UnityEngine;

namespace Client.States
{
    public class InitialState : IState
    {
        private const string GAME_SCENE_NAME = "LevelScene";
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly ISaveDataContainer saveDataContainer;
        private readonly LevelsLoopProgress loopProgress;

        private LoadingScreen loadingScreen;

        public InitialState(StateMachine stateMachine, ScreenNavigator screenNavigator, LevelsLoopProgress loopProgress, ISaveDataContainer saveDataContainer)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.loopProgress = loopProgress;
            this.saveDataContainer = saveDataContainer;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        public async UniTask Enter()
        {
            var facebookTask = LoadFb();

            saveDataContainer.Load();
            loopProgress.Load();
            // tutorialService.TryRunSteps();
            
            await screenNavigator.PushScreen<LoadingScreen>();
            await facebookTask;

            float fakeLoadingTime = 5;
#if !UNITY_EDITOR
            fakeLoadingTime = 5;
#endif
            stateMachine.Enter<LoadLevelState, LoadLevelState.Payload>(
                new LoadLevelState.Payload(loopProgress.GetNextLevelData(), fakeLoadingTime));
        }

        private async UniTask LoadFb()
        {
            var taskCompletionSource = new UniTaskCompletionSource<bool>();
            FB.Init(() => OnFbInitComplete(taskCompletionSource));
            
            await taskCompletionSource.Task;
            if(FB.IsInitialized)
                FB.ActivateApp();
        }

        private void OnFbInitComplete(UniTaskCompletionSource<bool> uniTaskCompletionSource)
        {
            uniTaskCompletionSource.TrySetResult(FB.IsInitialized);
        }

        private void UpdateProgress(float progress)
        {
            loadingScreen.SetProgress(progress);
        }
    }

    public class LoadingSceneState : IState
    {
        public UniTask Enter()
        {
            throw new System.NotImplementedException();
        }

        public UniTask Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}