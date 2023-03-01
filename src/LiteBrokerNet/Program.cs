using LiteBrokerNet;
using Task = LiteBrokerNet.Task;

Console.WriteLine("Hello World, LiteBrokerNet");

Console.WriteLine("Version from library is: " + LiteBroker.GetVersion());

var broker = new LiteBroker();

broker.Send("MyQueueFromCSharp", "{}");
var collection = broker.Receive();

Task lastTask = null;
foreach (var task in collection.Tasks)
{
    Console.WriteLine("Found task!");
    Console.WriteLine($"Id: {task.Id}");
    Console.WriteLine($"Queue: {task.Queue}");
    Console.WriteLine($"Payload: {task.Payload}");
    Console.WriteLine($"Created: {task.Created}");
    Console.WriteLine($"Status: {task.Status}");

    lastTask = task;
}

broker.SetStatus(lastTask.Id, 1);

collection.Dispose();

broker.Dispose();