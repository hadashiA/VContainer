namespace Zenject
{
    public interface IDecoratableMonoKernel
    {
        bool ShouldInitializeOnStart();
        void Initialize();
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void Dispose();
        void LateDispose();
    }

    public class DecoratableMonoKernel : IDecoratableMonoKernel
    {
        [InjectLocal] 
        public TickableManager TickableManager { get; protected set; } = null;

        [InjectLocal]
        public InitializableManager InitializableManager { get; protected set; } = null;

        [InjectLocal]
        public DisposableManager DisposablesManager { get; protected set; } = null;
        
        
        public virtual bool ShouldInitializeOnStart() => true;
        
        public virtual void Initialize()
        {
            InitializableManager.Initialize();
        }

        public void Update()
        {
            TickableManager.Update();
        }

        public void FixedUpdate()
        {
            TickableManager.FixedUpdate();
        }

        public void LateUpdate()
        {
            TickableManager.LateUpdate();
        }

        public void Dispose()
        {
            DisposablesManager.Dispose();
        }

        public void LateDispose()
        {
            DisposablesManager.LateDispose();
        }
    }

    public abstract class BaseMonoKernelDecorator : IDecoratableMonoKernel
    {
        [Inject] 
        protected IDecoratableMonoKernel DecoratedMonoKernel;

        public virtual bool ShouldInitializeOnStart() => DecoratedMonoKernel.ShouldInitializeOnStart();
        public virtual void Initialize() => DecoratedMonoKernel.Initialize();
        public virtual void Update() => DecoratedMonoKernel.Update();
        public virtual void FixedUpdate() => DecoratedMonoKernel.FixedUpdate();
        public virtual void LateUpdate() => DecoratedMonoKernel.LateUpdate();
        public virtual void Dispose() => DecoratedMonoKernel.Dispose();
        public virtual void LateDispose() => DecoratedMonoKernel.LateDispose();
    }
    
}