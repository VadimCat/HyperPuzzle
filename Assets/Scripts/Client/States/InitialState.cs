using Cysharp.Threading.Tasks;
using Facebook.Unity;
using Ji2Core.Core;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.UI.Screens;
using UI.Screens;

namespace Client.States
{
    public class InitialState : IState
    {
        private const string GAME_SCENE_NAME = "LevelScene";
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly SceneLoader sceneLoader;

        private LoadingScreen loadingScreen;
    
        public InitialState(StateMachine stateMachine, ScreenNavigator screenNavigator, SceneLoader sceneLoader)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.sceneLoader = sceneLoader;
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LoadingScreen>();
        }

        public async UniTask Enter()
        {
            var loadingTask = sceneLoader.LoadScene(GAME_SCENE_NAME);
            var facebookTask = LoadFb();
            loadingScreen = await screenNavigator.PushScreen<LoadingScreen>();
        
            // sceneLoader.OnProgressUpdate += UpdateProgress;
            await UniTask.WhenAll(loadingTask, facebookTask);
            
            
            // sceneLoader.OnProgressUpdate -= UpdateProgress;
        
            await screenNavigator.CloseScreen<LoadingScreen>();

            loadingScreen = null;

            stateMachine.Enter<GameState>();
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