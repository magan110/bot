# Implementation Plan

- [x] 1. Create core infrastructure and data layer





  - Set up .NET 8 Windows Forms project with proper folder structure (Models, Services, Repositories, Forms, Interfaces)
  - Define core interfaces: ISqlGenerator, IDatabaseRepository, ISqlValidator, IConfigurationService, IConversationService
  - Implement ConfigurationService for appsettings.json with secure API key storage using Windows DPAPI
  - Create DatabaseRepository with schema introspection using INFORMATION_SCHEMA and sys.* views
  - Implement schema caching to memory and schema.catalog.json with refresh functionality
  - Build SQL validation pipeline with chain of responsibility pattern for safety rules (SELECT-only, no semicolons, no dangerous functions, schema validation, row limits)
  - Add sys.sp_describe_first_result_set validation for SQL structure verification
  - Set up dependency injection, logging framework (Serilog), and comprehensive error handling
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 8.2, 8.3, 8.4, 8.5, 9.1, 9.2, 9.3, 9.4, 10.2, 10.4_

- [x] 2. Implement LLM integration and query processing system





  - Create ISqlGenerator interface with SqlGenerationResult model supporting Hinglish language detection
  - Implement OpenAI, Gemini, and Ollama providers with structured prompts for SQL generation
  - Build conversation context system with ConversationContext and session memory management
  - Create clarification question system for underspecified queries with bilingual support
  - Implement QueryOrchestrator to coordinate LLM generation → validation → execution workflow
  - Add parameterized query execution using Dapper ORM with DataTable result conversion
  - Create comprehensive logging for LLM prompts, SQL generation, and execution latency
  - Handle API errors, rate limiting, retry logic, and offline scenarios for all LLM providers
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 3.1, 3.2, 3.3, 3.4, 3.5, 5.1, 5.2, 5.3, 5.4, 5.5, 9.3, 10.1, 10.3_

- [x] 3. Build Windows Forms UI and complete application integration





  - Create MainForm with conversation panel (RichTextBox), input TextBox with Enter key handling, Ask button
  - Implement DataGridView for results with auto-sizing, context menu for copy/CSV export functionality
  - Add status bar, "SELECT-only enforced" security banner, and loading indicators
  - Create SettingsForm dialog for database connection, LLM provider selection, and MaxRows configuration
  - Implement rich text formatting for conversation history with user/system message differentiation
  - Add schema refresh menu item with background processing and progress indication
  - Wire all components through dependency injection with proper error handling and user feedback
  - Create comprehensive unit tests for validation rules, integration tests with real database and LLM providers
  - Add Hinglish language processing tests and security tests for SQL injection prevention
  - Build release configuration with installer package, application metadata, and user documentation
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 7.1, 7.2, 7.3, 7.4, 7.5, 8.1, 9.5, All requirements integration and validation_