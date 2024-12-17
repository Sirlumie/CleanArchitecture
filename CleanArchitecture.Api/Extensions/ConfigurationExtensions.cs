using System;
using CleanArchitecture.Domain.Rabbitmq;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Api.Extensions;

public static class ConfigurationExtensions
{
    public static RabbitMqConfiguration GetRabbitMqConfiguration(
        this IConfiguration configuration)
    {
        var isAspire = configuration["ASPIRE_ENABLED"] == "true";

        var rabbitEnabled = configuration["RabbitMQ:Enabled"];
        var rabbitHost = configuration["RabbitMQ:Host"];
        var rabbitPort = configuration["RabbitMQ:Port"];
        var rabbitUser = configuration["RabbitMQ:Username"];
        var rabbitPass = configuration["RabbitMQ:Password"];

        if (isAspire)
        {
            rabbitEnabled = "true";
            var connectionString = configuration["ConnectionStrings:RabbitMq"];

            var rabbitUri = new Uri(connectionString!);
            rabbitHost = rabbitUri.Host;
            rabbitPort = rabbitUri.Port.ToString();
            rabbitUser = rabbitUri.UserInfo.Split(':')[0];
            rabbitPass = rabbitUri.UserInfo.Split(':')[1];
        }

        return new RabbitMqConfiguration()
        {
            Host = rabbitHost ?? "",
            Port = int.Parse(rabbitPort ?? "0"),
            Enabled = bool.Parse(rabbitEnabled ?? "false"),
            Username = rabbitUser ?? "",
            Password = rabbitPass ?? ""
        };
    }
}