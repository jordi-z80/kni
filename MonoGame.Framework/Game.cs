// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;

using Microsoft.Xna.Platform;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is the entry point for most games. Handles setting up
    /// a window and graphics and runs a game loop that calls <see cref="Update"/> and <see cref="Draw"/>.
    /// </summary>
    public class Game : IDisposable
    {
        internal GameStrategy Strategy { get; private set; }

        private DrawableComponents _drawableComponents = new DrawableComponents();
        private UpdateableComponents _updateableComponents = new UpdateableComponents();

        private IGraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;

        private bool _initialized = false;

        /// <summary>
        /// Create a <see cref="Game"/>.
        /// </summary>
        public Game()
        {
            _instance = this;

            LaunchParameters = new LaunchParameters();

            Strategy = new ConcreteGame(this);

            Strategy.Activated += Platform_Activated;
            Strategy.Deactivated += Platform_Deactivated;
            Services.AddService(typeof(GameStrategy), Strategy);

        }

        ~Game()
        {
            Dispose(false);
        }

		[System.Diagnostics.Conditional("DEBUG")]
		internal void Log(string Message)
		{
			if (Strategy != null) Strategy.Log(Message);
		}

        void Platform_Activated(object sender, EventArgs e) { OnActivated(e); }
        void Platform_Deactivated(object sender, EventArgs e) { OnDeactivated(e); }

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            var handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose loaded game components
                    for (int i = 0; i < Strategy._components.Count; i++)
                    {
                        var disposable = Strategy._components[i] as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                    Strategy._components = null;

                    if (Strategy._content != null)
                    {
                        Strategy._content.Dispose();
                        Strategy._content = null;
                    }

                    if (_graphicsDeviceManager != null)
                    {
                        (_graphicsDeviceManager as GraphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }

                    if (Strategy != null)
                    {
                        Strategy.Activated -= Platform_Activated;
                        Strategy.Deactivated -= Platform_Deactivated;
                        Services.RemoveService(typeof(GameStrategy));

                        Strategy.Dispose();
                        Strategy = null;
                    }

                    AudioService.Shutdown();
                }
#if ANDROID
                Activity = null;
#endif
                _isDisposed = true;
                _instance = null;
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(
                    name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        #endregion IDisposable Implementation

        #region Properties

#if ANDROID
        [CLSCompliant(false)]
        public static AndroidGameActivity Activity { get; internal set; }
#endif
        private static Game _instance = null;
        internal static Game Instance { get { return Game._instance; } }

        /// <summary>
        /// The start up parameters for this <see cref="Game"/>.
        /// </summary>
        public LaunchParameters LaunchParameters { get; private set; }

        /// <summary>
        /// A collection of game components attached to this <see cref="Game"/>.
        /// </summary>
        public GameComponentCollection Components { get { return Strategy.Components; } }

        public TimeSpan InactiveSleepTime
        {
            get { return Strategy.InactiveSleepTime; }
            set { Strategy.InactiveSleepTime = value; }
        }

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime
        {
            get { return Strategy.MaxElapsedTime; }
            set { Strategy.MaxElapsedTime = value; }
        }

        /// <summary>
        /// Indicates if the game is the focused application.
        /// </summary>
        public bool IsActive
        {
            get { return Strategy.IsActive; }
        }


        public bool IsVisible
        {
            get { return Strategy.IsVisible; }
        }

        /// <summary>
        /// Indicates if the mouse cursor is visible on the game screen.
        /// </summary>
        public bool IsMouseVisible
        {
            get { return Strategy.IsMouseVisible; }
            set { Strategy.IsMouseVisible = value; }
        }

        /// <summary>
        /// The time between frames when running with a fixed time step. <seealso cref="IsFixedTimeStep"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Target elapsed time must be strictly larger than zero.</exception>
        public TimeSpan TargetElapsedTime
        {
            get { return Strategy.TargetElapsedTime; }
            set { Strategy.TargetElapsedTime = value; }
        }


        /// <summary>
        /// Indicates if this game is running with a fixed time between frames.
        /// 
        /// When set to <code>true</code> the target time between frames is
        /// given by <see cref="TargetElapsedTime"/>.
        /// </summary>
        public bool IsFixedTimeStep
        {
            get { return Strategy.IsFixedTimeStep; }
            set { Strategy.IsFixedTimeStep = value; }
        }

        /// <summary>
        /// Get a container holding service providers attached to this <see cref="Game"/>.
        /// </summary>
        public GameServiceContainer Services { get { return Strategy.Services; } }


        /// <summary>
        /// The <see cref="ContentManager"/> of this <see cref="Game"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">If Content is set to <code>null</code>.</exception>
        public ContentManager Content
        {
            get { return Strategy.Content; }
            set { Strategy.Content = value; }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> used for rendering by this <see cref="Game"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// There is no <see cref="Graphics.GraphicsDevice"/> attached to this <see cref="Game"/>.
        /// </exception>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_graphicsDeviceService == null)
                {
                    _graphicsDeviceService = (IGraphicsDeviceService)
                        Services.GetService(typeof(IGraphicsDeviceService));

                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");
                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }

        /// <summary>
        /// The system window that this game is displayed on.
        /// </summary>
        [CLSCompliant(false)]
        public GameWindow Window
        {
            get { return Strategy.Window; }
        }

        #endregion Properties

        #region Internal Properties

        // FIXME: Internal members should be eliminated.
        // Currently Game.Initialized is used by the Mac game window class to
        // determine whether to raise DeviceResetting and DeviceReset on
        // GraphicsDeviceManager.
        internal bool Initialized
        {
            get { return _initialized; }
        }

        #endregion Internal Properties

        #region Events

        /// <summary>
        /// Raised when the game gains focus.
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Raised when the game loses focus.
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// Raised when this game is being disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// Raised when this game is exiting.
        /// </summary>
        public event EventHandler<EventArgs> Exiting;

#if WINDOWS_UAP
        [CLSCompliant(false)]
        public Windows.ApplicationModel.Activation.ApplicationExecutionState PreviousExecutionState { get; internal set; }
#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Exit the game at the end of this tick.
        /// </summary>
        public void Exit()
        {
            Strategy.Exit();
        }

        /// <summary>
        /// Reset the elapsed game time to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public void ResetElapsedTime()
        {
            Strategy.ResetElapsedTime();
        }

        /// <summary>
        /// Supress calling <see cref="Draw"/> in the game loop.
        /// </summary>
        public void SuppressDraw()
        {
            Strategy.SuppressDraw();
        }
        
        /// <summary>
        /// Run the game for one frame, then exit.
        /// </summary>
        public void RunOneFrame()
        {
            if (Strategy == null)
                return;

            Strategy.RunOneFrame();
        }

        /// <summary>
        /// Run the game for the current platform.
        /// </summary>
        public void Run()
        {
            AssertNotDisposed();

            Strategy.Run();
        }

        /// <summary>
        /// Run the game. This method is valid only for the UAP/XAML template.
        /// </summary>
        internal void Run_UAP_XAML()
        {
            AssertNotDisposed();

            Strategy.Run_UAP_XAML();
        }

        internal void DoBeginRun()
        {
            BeginRun();
        }

        internal void DoEndRun()
        {
            EndRun();
        }
        
        /// <summary>
        /// Run one iteration of the game loop.
        ///
        /// Makes at least one call to <see cref="Update"/>
        /// and exactly one call to <see cref="Draw"/> if drawing is not supressed.
        /// When <see cref="IsFixedTimeStep"/> is set to <code>false</code> this will
        /// make exactly one call to <see cref="Update"/>.
        /// </summary>
        public void Tick()
        {
            Strategy.Tick();

            if (Strategy.ShouldExit)
                Strategy.TickExiting();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called right before <see cref="Draw"/> is normally called. Can return <code>false</code>
        /// to let the game loop not call <see cref="Draw"/>.
        /// </summary>
        /// <returns>
        ///   <code>true</code> if <see cref="Draw"/> should be called, <code>false</code> if it should not.
        /// </returns>
        protected virtual bool BeginDraw() { return true; }

        /// <summary>
        /// Called right after <see cref="Draw"/>. Presents the
        /// rendered frame in the <see cref="GameWindow"/>.
        /// </summary>
        protected virtual void EndDraw()
        {
            Strategy.EndDraw();
        }

        /// <summary>
        /// Called after <see cref="Initialize"/>, but before the first call to <see cref="Update"/>.
        /// </summary>
        protected virtual void BeginRun() { }

        /// <summary>
        /// Called when the game loop has been terminated before exiting.
        /// </summary>
        protected virtual void EndRun() { }

        /// <summary>
        /// Override this to load graphical resources required by the game.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        /// Override this to unload graphical resources loaded by the game.
        /// </summary>
        protected virtual void UnloadContent() { }

        /// <summary>
        /// Override this to initialize the game and load any needed non-graphical resources.
        ///
        /// Initializes attached <see cref="GameComponent"/> instances and calls <see cref="LoadContent"/>.
        /// </summary>
        protected virtual void Initialize()
        {
            // TODO: This should be removed once all platforms use the new GraphicsDeviceManager
#if ANDROID || IOS || TVOS
            // applyChanges
            {
                Strategy.BeginScreenDeviceChange(GraphicsDevice.PresentationParameters.IsFullScreen);

                if (GraphicsDevice.PresentationParameters.IsFullScreen)
                    Strategy.EnterFullScreen();
                else
                    Strategy.ExitFullScreen();
                var viewport = new Viewport(0, 0,
                                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                                            GraphicsDevice.PresentationParameters.BackBufferHeight);

                GraphicsDevice.Viewport = viewport;
                Strategy.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
            }
#endif

            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            // Initialize all existing components
            InitializeExistingComponents();

            _graphicsDeviceService = (IGraphicsDeviceService)
                Services.GetService(typeof(IGraphicsDeviceService));

            if (_graphicsDeviceService != null &&
                _graphicsDeviceService.GraphicsDevice != null)
            {
                LoadContent();
            }
        }

        /// <summary>
        /// Called when the game should draw a frame.
        ///
        /// Draws the <see cref="DrawableGameComponent"/> instances attached to this game.
        /// Override this to render your game.
        /// </summary>
        /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Draw"/> and the total time elapsed since the game started.</param>
        protected virtual void Draw(GameTime gameTime)
        {

            _drawableComponents.DrawVisibleComponents(gameTime);
        }

        /// <summary>
        /// Called when the game should update.
        ///
        /// Updates the <see cref="GameComponent"/> instances attached to this game.
        /// Override this to update your game.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="Update"/>.</param>
        protected virtual void Update(GameTime gameTime)
        {
            _updateableComponents.UpdateEnabledComponent(gameTime);
		}

        /// <summary>
        /// Called when the game is exiting. Raises the <see cref="Exiting"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Exiting"/> event.</param>
        protected virtual void OnExiting(EventArgs args)
        {
            var handler = Exiting;
            if (handler != null)
                handler(this, args);
        }
		
        /// <summary>
        /// Called when the game gains focus. Raises the <see cref="Activated"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Activated"/> event.</param>
		protected virtual void OnActivated(EventArgs args)
		{
			AssertNotDisposed();

            var handler = Activated;
            if (handler != null)
                handler(this, args);
		}
		
        /// <summary>
        /// Called when the game loses focus. Raises the <see cref="Deactivated"/> event.
        /// </summary>
        /// <param name="args">The arguments to the <see cref="Deactivated"/> event.</param>
		protected virtual void OnDeactivated(EventArgs args)
		{
			AssertNotDisposed();

            var handler = Deactivated;
            if (handler != null)
                handler(this, args);
		}

        #endregion Protected Methods

        #region Event Handlers

        private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            // Since we only subscribe to ComponentAdded after the graphics
            // devices are set up, it is safe to just blindly call Initialize.
            e.GameComponent.Initialize();

            if (e.GameComponent is IUpdateable)
                _updateableComponents.AddUpdatable((IUpdateable)e.GameComponent);
            if (e.GameComponent is IDrawable)
                _drawableComponents.AddDrawable((IDrawable)e.GameComponent);
        }

        private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent is IUpdateable)
                _updateableComponents.RemoveUpdatable((IUpdateable)e.GameComponent);
            if (e.GameComponent is IDrawable)
                _drawableComponents.RemoveDrawable((IDrawable)e.GameComponent);
        }

        #endregion Event Handlers

        #region Internal Methods

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();

            if (Strategy.BeforeUpdate(gameTime))
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();
				
                Update(gameTime);

                //The TouchPanel needs to know the time for when touches arrive
                TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;
            }
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();

            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            if (Strategy.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        internal void DoInitialize()
        {
            AssertNotDisposed();

            if (GraphicsDevice == null && graphicsDeviceManager != null)
                ((IGraphicsDeviceManager)graphicsDeviceManager).CreateDevice();

            Strategy.BeforeInitialize();
            Initialize();

            // We need to do this after virtual Initialize(...) is called.
            // 1. Categorize components into IUpdateable and IDrawable lists.
            // 2. Subscribe to Added/Removed events to keep the categorized
            //    lists synced and to Initialize future components as they are
            //    added.            

            _updateableComponents.ClearUpdatables();
            _drawableComponents.ClearDrawables();
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is IUpdateable)
                    _updateableComponents.AddUpdatable((IUpdateable)Components[i]);
                if (Components[i] is IDrawable)
                    _drawableComponents.AddDrawable((IDrawable)Components[i]);
            }

            Components.ComponentAdded += Components_ComponentAdded;
            Components.ComponentRemoved += Components_ComponentRemoved;

            _initialized = true;
        }

		internal void DoExiting()
		{
			OnExiting(EventArgs.Empty);
			UnloadContent();
		}

        #endregion Internal Methods

        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                if (_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager = (IGraphicsDeviceManager)
                        Services.GetService(typeof(IGraphicsDeviceManager));
                }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
        }

        // NOTE: InitializeExistingComponents really should only be called once.
        //       Game.Initialize is the only method in a position to guarantee
        //       that no component will get a duplicate Initialize call.
        //       Further calls to Initialize occur immediately in response to
        //       Components.ComponentAdded.
        private void InitializeExistingComponents()
        {
            for(int i = 0; i < Components.Count; ++i)
                Components[i].Initialize();
        }


        class DrawableComponents
        {
        
            private readonly List<IDrawable> _drawableComponents = new List<IDrawable>();
            private readonly List<IDrawable> _visibleComponents = new List<IDrawable>();
            private bool _isVisibleCacheInvalidated = true;

            private readonly List<DrawableJournalEntry> _addDrawableJournal = new List<DrawableJournalEntry>();
            private readonly List<int> _removeDrawableJournal = new List<int>();
            private int _addDrawableJournalCount;

            public void AddDrawable(IDrawable component)
            {
                // NOTE: We subscribe to item events after components in _addDrawableJournal have been merged.
                _addDrawableJournal.Add(new DrawableJournalEntry(component, _addDrawableJournalCount++));
                _isVisibleCacheInvalidated = true;
            }

            public void RemoveDrawable(IDrawable component)
            {
                if (_addDrawableJournal.Remove(new DrawableJournalEntry(component,-1)))
                    return;

                var index = _drawableComponents.IndexOf(component);
                if (index >= 0)
                {
                    component.VisibleChanged -= Component_VisibleChanged;
                    component.DrawOrderChanged -= Component_DrawOrderChanged;
                    _removeDrawableJournal.Add(index);
                    _isVisibleCacheInvalidated = true;
                }
            }

            public void ClearDrawables()
            {
                for (int i = 0; i < _drawableComponents.Count; i++)
                {
                    _drawableComponents[i].VisibleChanged -= Component_VisibleChanged;
                    _drawableComponents[i].DrawOrderChanged -= Component_DrawOrderChanged;
                }

                _addDrawableJournal.Clear();
                _removeDrawableJournal.Clear();
                _drawableComponents.Clear();
                _isVisibleCacheInvalidated = true;
            }

            private void Component_VisibleChanged(object sender, EventArgs e)
            {
                _isVisibleCacheInvalidated = true;
            }

            private void Component_DrawOrderChanged(object sender, EventArgs e)
            {
                var component = (IDrawable)sender;
                var index = _drawableComponents.IndexOf(component);

                _addDrawableJournal.Add(new DrawableJournalEntry(component, _addDrawableJournalCount++));

                component.VisibleChanged -= Component_VisibleChanged;
                component.DrawOrderChanged -= Component_DrawOrderChanged;
                _removeDrawableJournal.Add(index);

                _isVisibleCacheInvalidated = true;
            }

            internal void DrawVisibleComponents(GameTime gameTime)
            {
                if (_removeDrawableJournal.Count > 0)
                    ProcessRemoveDrawableJournal();
                if (_addDrawableJournal.Count > 0)
                    ProcessAddDrawableJournal();

                // rebuild _visibleComponents
                if (_isVisibleCacheInvalidated)
                {
                    _visibleComponents.Clear();
                    for (int i = 0; i < _drawableComponents.Count; i++)
                        if (_drawableComponents[i].Visible)
                            _visibleComponents.Add(_drawableComponents[i]);

                    _isVisibleCacheInvalidated = false;
                }

                // draw components
                for (int i = 0; i < _visibleComponents.Count; i++)
                    _visibleComponents[i].Draw(gameTime);

                // If the cache was invalidated as a result of processing components,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_isVisibleCacheInvalidated)
                    _visibleComponents.Clear();
            }

            private void ProcessRemoveDrawableJournal()
            {
                // Remove components in reverse.
                _removeDrawableJournal.Sort();
                for (int i = _removeDrawableJournal.Count - 1; i >= 0; i--)
                    _drawableComponents.RemoveAt(_removeDrawableJournal[i]);

                _removeDrawableJournal.Clear();
            }

            private void ProcessAddDrawableJournal()
            {
                // Prepare the _addJournal to be merge-sorted with _drawableComponents.
                // _drawableComponents is always sorted.
                _addDrawableJournal.Sort(DrawableJournalEntry.CompareAddJournalEntry);
                _addDrawableJournalCount = 0;

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _drawableComponents.Count && iAddJournal < _addDrawableJournal.Count)
                {
                    var addJournalItem = _addDrawableJournal[iAddJournal].Component;
                    // If addJournalItem is less than (belongs before) _items[iItems], insert it.
                    if (Comparer<int>.Default.Compare(addJournalItem.DrawOrder, _drawableComponents[iItems].DrawOrder) < 0)
                    {
                        addJournalItem.VisibleChanged += Component_VisibleChanged;
                        addJournalItem.DrawOrderChanged += Component_DrawOrderChanged;
                        _drawableComponents.Insert(iItems, addJournalItem);
                        iAddJournal++;
                    }
                    // Always increment iItems, either because we inserted and
                    // need to move past the insertion, or because we didn't
                    // insert and need to consider the next element.
                    iItems++;
                }

                // If _addJournal had any "tail" items, append them all now.
                for (; iAddJournal < _addDrawableJournal.Count; iAddJournal++)
                {
                    var addJournalItem = _addDrawableJournal[iAddJournal].Component;
                    addJournalItem.VisibleChanged += Component_VisibleChanged;
                    addJournalItem.DrawOrderChanged += Component_DrawOrderChanged;
                    _drawableComponents.Add(addJournalItem);
                }

                _addDrawableJournal.Clear();
            }

            private struct DrawableJournalEntry
            {
                private readonly int AddOrder;
                public readonly IDrawable Component;

                public DrawableJournalEntry(IDrawable component, int addOrder)
                {
                    Component = component;
                    this.AddOrder = addOrder;
                }

                public override int GetHashCode()
                {
                    return Component.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is DrawableJournalEntry))
                        return false;

                    return object.ReferenceEquals(Component, ((DrawableJournalEntry)obj).Component);
                }
                
                internal static int CompareAddJournalEntry(DrawableJournalEntry x, DrawableJournalEntry y)
                {
                    int result = Comparer<int>.Default.Compare(x.Component.DrawOrder, y.Component.DrawOrder);
                    if (result == 0)
                        result = x.AddOrder - y.AddOrder;
                    return result;
                }
            }
        }

        class UpdateableComponents
        {

            private readonly List<IUpdateable> _updateableComponents = new List<IUpdateable>();
            private readonly List<IUpdateable> _enabledComponents = new List<IUpdateable>();
            private bool _isEnabledCacheInvalidated = true;

            private readonly List<UpdateableJournalEntry> _addUpdateableJournal = new List<UpdateableJournalEntry>();
            private readonly List<int> _removeUpdateableJournal = new List<int>();
            private int _addUpdateableJournalCount;
        
            public void AddUpdatable(IUpdateable component)
            {
                // NOTE: We subscribe to item events after items in _addUpdateableJournal have been merged.
                _addUpdateableJournal.Add(new UpdateableJournalEntry(component, _addUpdateableJournalCount++));
                _isEnabledCacheInvalidated = true;
            }

            public void RemoveUpdatable(IUpdateable component)
            {
                if (_addUpdateableJournal.Remove(new UpdateableJournalEntry(component, -1)))
                    return;

                var index = _updateableComponents.IndexOf(component);
                if (index >= 0)
                {
                    component.EnabledChanged -= Component_EnabledChanged;
                    component.UpdateOrderChanged -= Component_UpdateOrderChanged;

                    _removeUpdateableJournal.Add(index);
                    _isEnabledCacheInvalidated = true;
                }
            }

            public void ClearUpdatables()
            {
                for (int i = 0; i < _updateableComponents.Count; i++)
                {                    
                    _updateableComponents[i].EnabledChanged -= Component_EnabledChanged;
                    _updateableComponents[i].UpdateOrderChanged -= Component_UpdateOrderChanged;
                }

                _addUpdateableJournal.Clear();
                _removeUpdateableJournal.Clear();
                _updateableComponents.Clear();

                _isEnabledCacheInvalidated = true;
            }

            private void Component_EnabledChanged(object sender, EventArgs e)
            {
                _isEnabledCacheInvalidated = true;
            }

            private void Component_UpdateOrderChanged(object sender, EventArgs e)
            {
                var component = (IUpdateable)sender;
                var index = _updateableComponents.IndexOf(component);

                _addUpdateableJournal.Add(new UpdateableJournalEntry(component, _addUpdateableJournalCount++));

                component.EnabledChanged -= Component_EnabledChanged;
                component.UpdateOrderChanged -= Component_UpdateOrderChanged;
                _removeUpdateableJournal.Add(index);

                _isEnabledCacheInvalidated = true;
            }
            
            private void ProcessRemoveUpdateableJournal()
            {
                // Remove components in reverse.
                _removeUpdateableJournal.Sort();
                for (int i = _removeUpdateableJournal.Count-1; i >= 0; i--)
                    _updateableComponents.RemoveAt(_removeUpdateableJournal[i]);

                _removeUpdateableJournal.Clear();
            }

            internal void UpdateEnabledComponent(GameTime gameTime)
            {
                if (_removeUpdateableJournal.Count > 0)
                    ProcessRemoveUpdateableJournal();
                if (_addUpdateableJournal.Count > 0)
                    ProcessAddUpdateableJournal();

                // rebuild _enabledComponents
                if (_isEnabledCacheInvalidated)
                {
                    _enabledComponents.Clear();
                    for (int i = 0; i < _updateableComponents.Count; i++)
                        if (_updateableComponents[i].Enabled)
                            _enabledComponents.Add(_updateableComponents[i]);

                    _isEnabledCacheInvalidated = false;
                }

                // update components
                for (int i = 0; i < _enabledComponents.Count; i++)
                    _enabledComponents[i].Update(gameTime);

                // If the cache was invalidated as a result of processing components,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_isEnabledCacheInvalidated)
                    _enabledComponents.Clear();
            }

            private void ProcessAddUpdateableJournal()
            {
                // Prepare the _addJournal to be merge-sorted with _updateableComponents.
                // _updateableComponents is always sorted.
                _addUpdateableJournal.Sort(UpdateableJournalEntry.CompareAddJournalEntry);
                _addUpdateableJournalCount = 0;

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _updateableComponents.Count && iAddJournal < _addUpdateableJournal.Count)
                {
                    var addJournalItem = _addUpdateableJournal[iAddJournal].Component;
                    // If addJournalItem is less than (belongs before) _items[iItems], insert it.
                    if (Comparer<int>.Default.Compare(addJournalItem.UpdateOrder, _updateableComponents[iItems].UpdateOrder) < 0)
                    {
                        addJournalItem.EnabledChanged += Component_EnabledChanged;
                        addJournalItem.UpdateOrderChanged += Component_UpdateOrderChanged;
                        _updateableComponents.Insert(iItems, addJournalItem);
                        iAddJournal++;
                    }
                    // Always increment iItems, either because we inserted and
                    // need to move past the insertion, or because we didn't
                    // insert and need to consider the next element.
                    iItems++;
                }

                // If _addJournal had any "tail" items, append them all now.
                for (; iAddJournal < _addUpdateableJournal.Count; iAddJournal++)
                {
                    var addJournalItem = _addUpdateableJournal[iAddJournal].Component;
                    addJournalItem.EnabledChanged += Component_EnabledChanged;
                    addJournalItem.UpdateOrderChanged += Component_UpdateOrderChanged;

                    _updateableComponents.Add(addJournalItem);
                }

                _addUpdateableJournal.Clear();
            }

            private struct UpdateableJournalEntry
            {
                private readonly int AddOrder;
                public readonly IUpdateable Component;

                public UpdateableJournalEntry(IUpdateable component, int addOrder)
                {
                    Component = component;
                    AddOrder = addOrder;
                }

                public override int GetHashCode()
                {
                    return Component.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is UpdateableJournalEntry))
                        return false;

                    return object.Equals(Component, ((UpdateableJournalEntry)obj).Component);
                }

                internal static int CompareAddJournalEntry(UpdateableJournalEntry x, UpdateableJournalEntry y)
                {
                    int result = Comparer<int>.Default.Compare(x.Component.UpdateOrder, y.Component.UpdateOrder);
                    if (result == 0)
                        result = x.AddOrder - y.AddOrder;
                    return result;
                }
            }
        }
    }
}
