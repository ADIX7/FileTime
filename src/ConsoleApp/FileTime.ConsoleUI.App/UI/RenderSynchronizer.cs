namespace FileTime.ConsoleUI.App.UI
{
    public class RenderSynchronizer
    {
        private readonly object renderLock = new();
        private readonly Application _application;
        private bool _needsRender;

        private CancellationTokenSource renderCancellationSource = new();

        public RenderSynchronizer(Application application)
        {
            _application = application;
        }
        public async void Start()
        {
            while (_application.IsRunning)
            {
                if (_needsRender)
                {
                    lock (renderLock)
                    {
                        _needsRender = false;
                        renderCancellationSource = new();
                    }

                    await _application.PrintUI(renderCancellationSource.Token);
                }
            }
        }

        public void NeedsReRender()
        {
            lock (renderLock)
            {
                _needsRender = true;
                if (renderCancellationSource.Token.CanBeCanceled)
                {
                    renderCancellationSource.Cancel();
                }
            }
        }
    }
}