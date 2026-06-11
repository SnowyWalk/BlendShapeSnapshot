namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal interface IEditorWindowModule
    {
        public void Initialize(IEditorWindowOrchestrator orchestrator);
        public void OnEnable();
        public void OnDisable();
    }
}
