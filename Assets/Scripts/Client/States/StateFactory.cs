using System;
using System.Collections.Generic;
using Client.Views.Level;
// using Client.Presenters;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.States;
using UI.Background;

namespace Client.States
{
    public class StateFactory : IStateFactory
    {
        private readonly Context context;

        public StateFactory(Context context)
        {
            this.context = context;
        }

        public Dictionary<Type, IExitableState> GetStates(StateMachine stateMachine)
        {
            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(InitialState)] = new InitialState(stateMachine,
                context.GetService<Ji2Core.Core.ScreenNavigation.ScreenNavigator>(),
                context.GetService<LevelsLoopProgress>(), context.SaveDataContainer);

            dict[typeof(LoadLevelState)] = new LoadLevelState(stateMachine, context, context.SceneLoader(),
                context.ScreenNavigator, context.GetService<LevelsConfig>(), context.GetService<BackgroundService>());

            dict[typeof(GameState)] = new GameState(stateMachine, context.ScreenNavigator);
            
            dict[typeof(LevelCompletedState)] = new LevelCompletedState(stateMachine, context.ScreenNavigator,
                context.LevelsLoopProgress, context.GetService<LevelsConfig>(), context.GetService<AudioService>());

            return dict;
        }
    }
}