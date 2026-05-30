using System.Collections.Generic;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class GPUResourceDeletionTests
{
    private sealed class FakeGPUResourceDeletionService : IGPUResourceDeletionService
    {
        public List<int> ShadersQueued { get; } = [];
        public List<int> TexturesQueued { get; } = [];
        public List<int> VertexArraysQueued { get; } = [];
        public List<int> VBOsQueued { get; } = [];
        public List<int> EBOsQueued { get; } = [];
        public List<int> FramebuffersQueued { get; } = [];
        public int ProcessDeletesCallCount { get; private set; } = 0;

        public void QueueShaderDelete(int handle) => ShadersQueued.Add(handle);

        public void QueueTextureDelete(int handle) => TexturesQueued.Add(handle);

        public void QueueVertexArrayDelete(int handle) => VertexArraysQueued.Add(handle);

        public void QueueVBODelete(int handle) => VBOsQueued.Add(handle);

        public void QueueEBODelete(int handle) => EBOsQueued.Add(handle);

        public void QueueFramebufferDelete(int handle) => FramebuffersQueued.Add(handle);

        public void ProcessDeletes() => ProcessDeletesCallCount++;
    }

    private FakeGPUResourceDeletionService _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeGPUResourceDeletionService();
        EngineConfiguration.GPUResourceDeletionService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void TryRequestDeleteFBO_NonZeroHandle_EnqueuesFramebufferDelete()
    {
        bool accepted = GPUResourceDeletion.TryRequestDeleteFBO(42);

        Assert.IsTrue(accepted);
        Assert.HasCount(1, _fake.FramebuffersQueued);
        Assert.AreEqual(42, _fake.FramebuffersQueued[0]);
    }

    [TestMethod]
    public void TryRequestDeleteFBO_ZeroHandle_DoesNotEnqueueButReturnsTrue()
    {
        bool accepted = GPUResourceDeletion.TryRequestDeleteFBO(0);

        Assert.IsTrue(accepted);
        Assert.IsEmpty(_fake.FramebuffersQueued);
    }

    [TestMethod]
    public void TryRequestDeleteFBO_MultipleHandles_AllEnqueuedInOrder()
    {
        GPUResourceDeletion.TryRequestDeleteFBO(7);
        GPUResourceDeletion.TryRequestDeleteFBO(11);
        GPUResourceDeletion.TryRequestDeleteFBO(13);

        Assert.HasCount(3, _fake.FramebuffersQueued);
        Assert.AreEqual(7, _fake.FramebuffersQueued[0]);
        Assert.AreEqual(11, _fake.FramebuffersQueued[1]);
        Assert.AreEqual(13, _fake.FramebuffersQueued[2]);
    }

    [TestMethod]
    public void TryRequestDeleteFBO_DoesNotTouchOtherQueues()
    {
        GPUResourceDeletion.TryRequestDeleteFBO(99);

        Assert.IsEmpty(_fake.ShadersQueued);
        Assert.IsEmpty(_fake.TexturesQueued);
        Assert.IsEmpty(_fake.VertexArraysQueued);
        Assert.IsEmpty(_fake.VBOsQueued);
        Assert.IsEmpty(_fake.EBOsQueued);
    }
}
