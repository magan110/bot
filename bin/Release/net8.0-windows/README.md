# DbAutoChat.Win

A .NET 8 Windows Forms application that enables natural language querying of SQL Server databases with comprehensive safety validation and multi-LLM provider support.

## Features

- **Natural Language Processing**: Convert English and Hinglish queries to SQL
- **Multi-LLM Support**: OpenAI, Gemini, and Ollama providers
- **Database Schema Introspection**: Automatic discovery and caching of database structure
- **SQL Safety Validation**: Comprehensive validation pipeline ensuring read-only operations
- **Conversation Context**: Session-aware query processing with clarification support
- **Rich UI**: Windows Forms interface with conversation history and data grid results
- **Export Functionality**: Copy to clipboard and CSV export capabilities
- **Security Features**: SELECT-only enforcement, SQL injection prevention, encrypted API key storage

## Architecture

The application follows a layered architecture with clear separation of concerns:

- **Presentation Layer**: Windows Forms UI components (MainForm, SettingsForm)
- **Application Layer**: Query orchestration and workflow management (QueryOrchestrator)
- **Domain Layer**: Business logic and core entities (Models, Validation)
- **Infrastructure Layer**: Database and LLM provider integrations
- **Cross-Cutting**: Logging, configuration, and validation

## User Interface

### Main Window
- **Conversation Panel**: Rich text display of user queries and system responses
- **Input Panel**: Text box for natural language queries with Enter key support
- **Results Panel**: DataGridView with auto-sizing columns and context menu
- **Security Banner**: Visual indicator of read-only mode enforcement
- **Status Bar**: Connection status, query progress, and row count display
- **Menu System**: Settings, schema refresh, and help options

### Settings Dialog
- **Database Tab**: Connection string configuration and testing
- **LLM Provider Tab**: Provider selection (OpenAI/Gemini/Ollama) and API key management
- **General Tab**: Maximum row limits and other preferences

## Core Components

### LLM Integration
- `ISqlGenerator` interface with provider implementations
- `SqlGenerationResult` model supporting parameterized queries
- Language detection for English/Hinglish input
- Conversation context management with session variables

### Database Layer
- `IDatabaseRepository` for schema introspection and query execution
- Schema caching with JSON persistence (`schema.catalog.json`)
- Dapper ORM integration for efficient data access
- Connection management and comprehensive error handling

### Validation Pipeline
- Chain of responsibility pattern for SQL validation
- SELECT-only enforcement with DML/DDL blocking
- Schema validation against cached metadata
- Dangerous function and injection prevention (xp_cmdshell, etc.)
- Row limit enforcement with automatic TOP clause injection
- SQL structure validation using `sys.sp_describe_first_result_set`

### Configuration Management
- JSON-based configuration with secure API key storage
- Windows DPAPI encryption for sensitive data
- Runtime configuration updates through Settings dialog
- Provider-specific settings management

## Getting Started

### Prerequisites
- .NET 8.0 SDK or Runtime
- SQL Server (any edition, including Express)
- LLM Provider API key (OpenAI, Gemini, or local Ollama installation)

### Installation
1. Download the latest release or clone the repository
2. If building from source: `dotnet build --configuration Release`
3. Configure database connection and LLM provider in Settings
4. Run the application

### Quick Setup
1. Launch DbAutoChat.Win
2. Go to Tools → Settings
3. Configure your SQL Server connection string
4. Select your preferred LLM provider and enter API key
5. Test the connection and save settings
6. Start asking questions about your database!

## Configuration

### Database Connection
```
Server=localhost;Database=YourDB;Integrated Security=true;TrustServerCertificate=true;
```

### LLM Providers

**OpenAI**
- API Key: Your OpenAI API key (sk-...)
- Model: gpt-4 (default) or gpt-3.5-turbo

**Gemini**
- API Key: Your Google AI Studio API key (AIza...)
- Model: gemini-pro (default)

**Ollama**
- Base URL: http://localhost:11434 (default)
- Model: llama2 or any installed model

## Usage Examples

### English Queries
- "Show me all customers from Delhi"
- "Top 10 orders by revenue this month"
- "Customer details for order BW438101"
- "Sales data for the last quarter"
- "Products with inventory below 100 units"

### Hinglish Queries  
- "Delhi ke sabse zyada sales wale customers dikhao"
- "Is month ke top 10 orders revenue ke hisab se"
- "BW438101 order ke customer ki details chahiye"
- "Last quarter ka sales data batao"
- "100 se kam inventory wale products dikhao"

### Clarification Handling
The system will ask for clarification when queries are ambiguous:
- **User**: "Show me sales"
- **System**: "Which time period are you interested in? Today, this week, this month, or a specific date range?"

## Security Features

- **SELECT-only enforcement**: Only read operations permitted, all DML/DDL blocked
- **SQL injection prevention**: Parameterized queries and comprehensive validation
- **Schema validation**: Queries validated against actual database structure
- **Row limits**: Configurable maximum result set size (default: 1000 rows)
- **API key encryption**: Secure storage using Windows Data Protection API (DPAPI)
- **Dangerous function blocking**: Prevents execution of system functions like xp_cmdshell
- **Comment and semicolon blocking**: Prevents SQL comment injection and statement chaining

## Data Export

### Copy to Clipboard
- Select rows in the results grid
- Right-click → Copy Selected
- Data is copied in tab-delimited format

### CSV Export
- Right-click in results grid → Export to CSV
- Choose filename and location
- Full dataset exported with proper CSV escaping

## Testing

The application includes comprehensive test suites:

### Unit Tests (`Test/SqlValidationTests.cs`)
- SELECT-only validation
- SQL injection prevention
- Schema validation
- Parameter validation
- Security rule enforcement

### Integration Tests (`Test/LlmIntegrationTest.cs`)
- LLM provider integration
- Conversation context management
- Language detection (English/Hinglish)
- Query orchestration workflow

### Security Tests
- SQL injection attempt blocking
- Dangerous function prevention
- Schema boundary enforcement

Run in Debug mode to execute all tests automatically on startup.

## Troubleshooting

### Common Issues

**Connection Failed**
- Verify SQL Server is running and accessible
- Check connection string format
- Ensure database exists and user has read permissions
- For Windows Authentication, run application as appropriate user

**LLM Provider Errors**
- Verify API key is correct and has sufficient credits/quota
- Check internet connectivity for cloud providers
- For Ollama, ensure service is running on specified port

**Schema Refresh Issues**
- Verify database permissions for INFORMATION_SCHEMA access
- Check for network connectivity issues
- Try manual schema refresh from Tools menu

**Query Validation Errors**
- Ensure queries reference existing tables and columns
- Check for unsupported SQL constructs (non-SELECT statements)
- Verify parameter formatting

## Building from Source

### Development Setup
```bash
git clone <repository-url>
cd DbAutoChat.Win
dotnet restore
dotnet build
```

### Release Build
```powershell
.\build-release.ps1 -CreateInstaller
```

This creates a release build and ZIP package for distribution.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Add tests for new functionality
4. Ensure all tests pass
5. Update documentation as needed
6. Submit a pull request

### Development Guidelines
- Follow existing code style and patterns
- Add comprehensive tests for new features
- Update documentation for user-facing changes
- Ensure security validation for any SQL-related changes

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions:
- Create an issue in the repository
- Check existing documentation and troubleshooting guide
- Review test cases for expected behavior examples