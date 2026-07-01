using System;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public static class BlendShapeSnapshotComposition
    {
        public static BlendShapeSnapshotPresenter Create(Action repaint)
        {
            SnapshotRepository repository = new SnapshotRepository();
            SnapshotListModel listModel = new SnapshotListModel(repository);
            SnapshotDiffService diffService = new SnapshotDiffService(repository);
            BlendShapeSnapshotService snapshotService = new BlendShapeSnapshotService();
            SnapshotPreviewRenderer previewRenderer = new SnapshotPreviewRenderer(repaint);
            BlendShapeSnapshotView view = new BlendShapeSnapshotView();

            return new BlendShapeSnapshotPresenter(
                repository,
                listModel,
                diffService,
                snapshotService,
                previewRenderer,
                view,
                repaint);
        }
    }
}
