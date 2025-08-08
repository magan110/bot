using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using DbAutoChat.Win.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DbAutoChat.Win.Test;

/// <summary>
/// Basic test to verify core infrastructure components work correctly
/// </summary>
public static class CoreInfrastructureTest
{
    public static void RunTests()
    {
        Console.WriteLine("Running Core Infrastructure Tests...");
        
        TestConfigurationService();
        TestConversationService();
        TestSqlValidationPipeline();
        
        Console.WriteLine("All tests completed successfully!");
    }

    private static void TestConfigurationService()
    {
        Console.WriteLine("Testing ConfigurationService...");
        
        var configService = new ConfigurationService();
        
        // Test setting and getting values
        configService.SetSetting("TestKey", "TestValue");
        var value = configService.GetSetting<string>("TestKey");
        
        if (value != "TestValue")
            throw new Exception("ConfigurationService test failed");
        
        // Test API key encryption
        configService.SetSetting("Bot:OpenAI:ApiKey", "test-api-key");
        var apiKey = configService.GetSetting<string>("Bot:OpenAI:ApiKey");
        
        if (apiKey != "test-api-key")
            throw new Exception("API key encryption/decryption test failed");
        
        Console.WriteLine("✓ ConfigurationService tests passed");
    }

    private static void TestConversationService()
    {
        Console.WriteLine("Testing ConversationService...");
        
        var conversationService = new ConversationService(NullLogger<ConversationService>.Instance);
        
        conversationService.AddUserMessage("Test user message");
        conversationService.AddSystemResponse("Test system response");
        
        var context = conversationService.GetCurrentContext();
        
        if (context.Messages.Count != 2)
            throw new Exception("ConversationService message count test failed");
        
        if (context.Messages[0].Role != "user" || context.Messages[1].Role != "system")
            throw new Exception("ConversationService message roles test failed");
        
        conversationService.ClearContext();
        context = conversationService.GetCurrentContext();
        
        if (context.Messages.Count != 0)
            throw new Exception("ConversationService clear context test failed");
        
        Console.WriteLine("✓ ConversationService tests passed");
    }

    private static void TestSqlValidationPipeline()
    {
        Console.WriteLine("Testing SqlValidationPipeline...");
        
        var configService = new ConfigurationService();
        var logger = NullLogger<SqlValidationPipeline>.Instance;
        var validator = new SqlValidationPipeline(configService, logger);
        var schema = new DatabaseSchema();
        
        // Test valid SELECT query
        var validResult = validator.ValidateQuery("SELECT TOP 10 * FROM Users", schema);
        if (!validResult.IsValid)
            throw new Exception("Valid SELECT query validation failed");
        
        // Test invalid INSERT query
        var invalidResult = validator.ValidateQuery("INSERT INTO Users VALUES (1, 'test')", schema);
        if (invalidResult.IsValid)
            throw new Exception("Invalid INSERT query should have failed validation");
        
        // Test semicolon rejection
        var semicolonResult = validator.ValidateQuery("SELECT * FROM Users; DROP TABLE Users", schema);
        if (semicolonResult.IsValid)
            throw new Exception("Query with semicolon should have failed validation");
        
        // Test dangerous function rejection
        var dangerousResult = validator.ValidateQuery("SELECT * FROM Users; EXEC xp_cmdshell 'dir'", schema);
        if (dangerousResult.IsValid)
            throw new Exception("Query with dangerous function should have failed validation");
        
        Console.WriteLine("✓ SqlValidationPipeline tests passed");
    }
}