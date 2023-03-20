using Client.States;
using Client.Views.Level;
using Core.Compliments;
using Ji2.CommonCore;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Models.Analytics;
using Ji2.UI;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.Core.UserInput;
using Ji2Core.Plugins.AppMetrica;
using Models;
using UI.Background;
using UnityEngine;

namespace Client
{
    public class Bootstrap : BootstrapBase
    {
        [SerializeField] private LevelsConfig levelsConfig;

        [SerializeField] private ScreenNavigator screenNavigator;
        [SerializeField] private BackgroundService backgroundService;
        [SerializeField] private UpdateService updateService;
        [SerializeField] private TextCompliments complimentsWordsService;
        [SerializeField] private AudioService audioService;
        [SerializeField] private TutorialPointerView tutorialPointerView;

        private AppSession appSession;

        private readonly Context context = Context.GetInstance();

        //TODO: CREATE COMMON CORE INSTALLER/INSTALLERS
        protected override void Start()
        {
            DontDestroyOnLoad(this);
            //TODO: Create installers where needed
            InstallCamera();
            InstallAudioService();
            InstallLevelsData();
            InstallNavigator();
            InstallInputService();
            InstallSaveDataContainer();
            InstallUpdatesService();
            InstallAnalytics();
            InstallSceneLoader();
            InstallCompliments();
            InstallBackgrounds();
            
            levelsConfig.Bootstrap();
            context.Register(levelsConfig);
            
            context.Register(new LevelsLoopProgress(context.SaveDataContainer, levelsConfig.GetLevelsOrder()));
            
            var appStateMachine = InstallStateMachine();
            // InstallTutorial();

            // context.Register(levelConfig);

            StartApp(appStateMachine);
        }

        private void InstallTutorial()
        {
            context.Register(tutorialPointerView);
            // var tutorial = new TutorialService(context.SaveDataContainer,
            //     new[] { new TutorialStepsFactory(context).Create<InitialTutorialState>() });
            // context.Register(tutorial);
        }

        private void StartApp(StateMachine appStateMachine)
        {
            appStateMachine.Load();
            appSession = new AppSession(appStateMachine);
            appSession.StateMachine.Enter<InitialState>();
        }

        private StateMachine InstallStateMachine()
        {
            StateMachine appStateMachine = new StateMachine(new StateFactory(context));
            context.Register(appStateMachine);
            return appStateMachine;
            return null;
        }

        private void InstallBackgrounds()
        {
            context.Register(backgroundService);
        }

        private void InstallCompliments()
        {
            context.Register<ICompliments>(complimentsWordsService);
        }

        private void InstallSceneLoader()
        {
            var sceneLoader = new SceneLoader(updateService);
            context.Register(sceneLoader);
        }

        private void InstallAnalytics()
        {
            IAnalytics analytics = new Analytics();
            analytics.AddLogger(new YandexMetricaLogger(AppMetrica.Instance));
            context.Register<IAnalytics>(analytics);
        }

        private void InstallUpdatesService()
        {
            context.Register(updateService);
        }

        private void InstallSaveDataContainer()
        {
            ISaveDataContainer saveDataContainer = new PlayerPrefsSaveDataContainer();
            context.Register<ISaveDataContainer>(saveDataContainer);
        }

        private void InstallCamera()
        {
            context.Register(new CameraProvider());
        }

        private void InstallInputService()
        {
            context.Register(new InputService(updateService));
        }

        private void InstallAudioService()
        {
            audioService.Bootstrap();
            // audioService.PlayMusic(SoundNamesCollection.Music);
            context.Register(audioService);
        }

        private void InstallNavigator()
        {
            screenNavigator.Bootstrap();
            context.Register(screenNavigator);
        }

        private void InstallLevelsData()
        {
            // levelsStorageBase.Bootstrap();
        }
    }
}