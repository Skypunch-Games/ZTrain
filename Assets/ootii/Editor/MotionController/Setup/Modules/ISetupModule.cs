using com.ootii.Actors.AnimationControllers;

namespace com.ootii.Setup.Modules
{
    public interface ISetupModule
    {
        int Priority { get; }
        string Category { get; }
        bool IsValid { get; }

        void Initialize(bool rUseDefaults);
        void BeginSetup(MotionController rMotionController);        
    }
}
