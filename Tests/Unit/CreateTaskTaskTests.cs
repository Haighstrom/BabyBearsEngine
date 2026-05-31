using BabyBearsEngine.Tasks;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class CreateTaskTaskTests
{
    [TestMethod]
    public void Start_SetsCreatedTaskAsNextTask()
    {
        var inserted = new Task();
        var splicer = new CreateTaskTask(() => inserted);

        splicer.Start();

        Assert.AreSame(inserted, splicer.NextTask);
    }

    [TestMethod]
    public void Start_WithExistingNextTask_ReAttachesItBehindTheInsertedTask()
    {
        var inserted = new Task();
        var originalNext = new Task();
        var splicer = new CreateTaskTask(() => inserted)
        {
            NextTask = originalNext,
        };

        splicer.Start();

        Assert.AreSame(inserted, splicer.NextTask);
        Assert.AreSame(originalNext, inserted.NextTask);
    }

    [TestMethod]
    public void Start_WithNoExistingNextTask_LeavesInsertedTaskNextNull()
    {
        var inserted = new Task();
        var splicer = new CreateTaskTask(() => inserted);

        splicer.Start();

        Assert.IsNull(inserted.NextTask);
    }

    [TestMethod]
    public void Start_InvokesTaskCreatorExactlyOnce()
    {
        int creatorCalls = 0;
        var splicer = new CreateTaskTask(() =>
        {
            creatorCalls++;
            return new Task();
        });

        splicer.Start();

        Assert.AreEqual(1, creatorCalls);
    }

    [TestMethod]
    public void Update_AutoCompletes_AfterSplice()
    {
        // CreateTaskTask has no completion conditions of its own, so the moment Start runs it
        // is already IsComplete and the next Update tick will Complete it.
        var inserted = new Task();
        var splicer = new CreateTaskTask(() => inserted);
        bool completed = false;
        splicer.TaskCompleted += (_, _) => completed = true;

        splicer.Update(0.016);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void Update_PerformsSpliceBeforeAutoComplete()
    {
        var inserted = new Task();
        var splicer = new CreateTaskTask(() => inserted);

        splicer.Update(0.016);

        Assert.AreSame(inserted, splicer.NextTask);
    }
}
