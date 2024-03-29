// See https://aka.ms/new-console-template for more information

using MassTransit;
using MassTransit.Caching;
using MassTransit.MessageSending;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var services = new ServiceCollection();

services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri($"amqp://localhost:5672"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.Send<NotificationMessage>(x => x.UseRoutingKeyFormatter(new CustomRoutingKeyFormatter<NotificationMessage>()));
        cfg.Message<NotificationMessage>(x => x.SetEntityName("notification"));
        cfg.Publish<NotificationMessage>(x =>
            {
                x.ExchangeType = "direct";
                x.Exclude = true;
            }
        );

        cfg.Send<OutgoingNotificationMessage>(x => x.UseRoutingKeyFormatter(new CustomRoutingKeyFormatter<OutgoingNotificationMessage>()));
        cfg.Message<OutgoingNotificationMessage>(x => x.SetEntityName("notification"));
        cfg.Publish<OutgoingNotificationMessage>(x => x.ExchangeType = "direct");

        cfg.ConfigureEndpoints(context);
    });

});
//services.AddOptions<MassTransitHostOptions>()
//    .Configure(options =>
//    {
//        options.WaitUntilStarted = true;
//        options.StartTimeout = TimeSpan.FromSeconds(30);
//        options.StopTimeout = TimeSpan.FromSeconds(60);
//    });
//services.AddOptions<HostOptions>()
//    .Configure(options =>
//    {
//        options.StartupTimeout = TimeSpan.FromSeconds(60);
//        options.ShutdownTimeout = TimeSpan.FromSeconds(60);
//    });


var provider = services.BuildServiceProvider();
var busControl = provider.GetRequiredService<IBusControl>();
var bus = provider.GetRequiredService<IBus>();

var key = string.Empty;
do
{
    Console.Write("Select Queue | 1- notify, 2- outgoing notify : ");
    var queueNumber = Console.ReadLine();
    var queueName = queueNumber == "1" ? "queue.direct.notify" : "queue.direct.notify.outgoing";

    //var endPoint = await bus.GetSendEndpoint(new Uri($"queue:{queueName}"));
    var message = new NotificationMessage
    {
        AgentId = 1,
        GWReferenceNo = "11616_20240222173002021d56be_0_10_anl137",
        NotificationType = 1,
        Payload = "{\"OutgoingRequestLogID\":17236145,\"AgentID\":11667,\"ProductID\":0,\"ReferanceNo\":\"11616_20240222173002021d56be_0_10_anl137\",\"Action\":\"PoliceAra-DyanmicProductOutsourceProcess-163-DogaSigorta\",\"RequestDate\":\"2024-03-11T20:35:38.7172918+03:00\",\"ElapsedTime\":0,\"DataReceived\":null,\"DataSent\":\"H4sIAAAAAAAEAG2SXWuDMBiF7wf7D877NVZtK+ICHRu0CGNQxq7fanAviSbEWOe/n1btRHuXnHOek8+olKDC9+LChFTM+s1FUYad9mL/GKNCQuq6XtXeSuqMuI7jEWdDOv+ZDYw9QIblN6Ydq0rjFbLp44NlWdF1oQODlGkylV5l2vTzTmrJ8FMKTNhew00eHV4JAQUmuE+RvskMvtn5hIZ9aWYwj8giMy9QoKUAeoi1yc+7tbdxkfOnHhy8OVJiJrUB0fWdZAMpkkXGJDHmAvmHpP422K7dnR8Efl878ebYhekMR3fZqgRwOAou04oOW5wod9PT3DJxvdd7K3Fo0BxVe8q4rf73+7bZe0Rk+m7DbPxB9A8as43iUQIAAA==\",\"RequestID\":7866198,\"ProposalID\":null}",
    };

    //await endPoint.Send(message);

    try
    {
        // Exchange adı ve route key
        var exchangeName = "exchange.direct.notify";
        var routeKey = "direct.notify.outgoing";

        // Exchange adresini oluşturma
        var address = new Uri($"exchange:{exchangeName}?bind=true&routingKey={routeKey}");

        // SendEndpoint almak
        var sendEndpoint = await busControl.GetSendEndpoint(new Uri($"queue:common.notification"));

        // Mesajı göndermek
        //await sendEndpoint.Send<NotificationMessage>(message);

        await busControl.Publish<OutgoingNotificationMessage>(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    Console.WriteLine($"message sent to {queueName} queue");

    Console.WriteLine("Please prees to 'Q' for exit");
    key = Console.ReadKey().ToString();

    Console.Clear();
}
while (key != "Q");



