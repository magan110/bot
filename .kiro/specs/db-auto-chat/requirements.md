# Requirements Document

## Introduction

DbAutoChat.Win is a .NET 8 Windows Forms desktop application that enables users to query SQL Server databases using natural language questions. The application automatically converts natural language queries into safe, read-only SQL SELECT statements and displays results in a user-friendly interface. The system includes schema introspection, pluggable LLM providers, comprehensive SQL safety validation, and conversational context management.

## Requirements

### Requirement 1

**User Story:** As a database user, I want to ask natural language questions about my SQL Server data in English or Hinglish, so that I can quickly get insights without writing SQL queries.

#### Acceptance Criteria

1. WHEN the user enters a natural language question in English or Hinglish THEN the system SHALL convert it to a SQL SELECT statement using the cached schema
2. WHEN the user asks questions like "show sales in Jaipur last month" or "Jaipur mein last month ke sales dikhao" THEN the system SHALL generate appropriate SQL using date functions and location filters
3. WHEN the user asks "top 10 customers by revenue this quarter" or "is quarter ke top 10 customers revenue ke hisab se" THEN the system SHALL generate SQL with TOP clause and appropriate aggregations
4. WHEN the user asks "orders for BW438101 today" or "BW438101 ke liye aaj ke orders" THEN the system SHALL generate SQL with parameterized values and date filtering
5. WHEN the generated SQL is valid THEN the system SHALL execute it and display results in a DataGridView
6. WHEN the user mixes Hindi and English words in their query THEN the system SHALL understand and process Hinglish input correctly

### Requirement 2

**User Story:** As a database administrator, I want the application to automatically discover and cache my database schema, so that queries can reference actual tables and columns.

#### Acceptance Criteria

1. WHEN the application starts THEN the system SHALL introspect the database schema using INFORMATION_SCHEMA and sys.* views
2. WHEN schema introspection completes THEN the system SHALL cache table names, column names, data types, primary keys, and foreign key relationships in memory
3. WHEN schema is cached THEN the system SHALL save it to schema.catalog.json file for persistence
4. WHEN the user triggers "Refresh Schema" THEN the system SHALL re-introspect and update the cached schema
5. WHEN schema introspection fails THEN the system SHALL display an error message and prevent query execution

### Requirement 3

**User Story:** As a system integrator, I want to use different LLM providers for natural language processing, so that I can choose the best provider for my needs.

#### Acceptance Criteria

1. WHEN configuring the system THEN the user SHALL be able to select from OpenAI, Gemini, or Ollama providers
2. WHEN using any LLM provider THEN the system SHALL implement the ISqlGenerator interface consistently
3. WHEN sending requests to LLM THEN the system SHALL use a strict system prompt that enforces SELECT-only queries and supports both English and Hinglish input
4. WHEN the LLM generates SQL THEN the system SHALL ensure all literals are parameterized with named parameters
5. WHEN the generated SQL lacks TOP clause THEN the system SHALL automatically inject TOP {MaxRows}

### Requirement 4

**User Story:** As a security-conscious user, I want all SQL queries to be validated for safety, so that only read-only operations are permitted.

#### Acceptance Criteria

1. WHEN any SQL is generated THEN the system SHALL reject non-SELECT statements (INSERT, UPDATE, DELETE, etc.)
2. WHEN SQL contains DML or DDL operations THEN the system SHALL block execution and show an error
3. WHEN SQL contains semicolons, comments, or dangerous functions like xp_cmdshell THEN the system SHALL reject the query
4. WHEN SQL references unknown tables or columns THEN the system SHALL validate against the cached schema and reject if invalid
5. WHEN SQL would return more than the configured row limit THEN the system SHALL enforce the maximum row cap
6. WHEN SQL is generated THEN the system SHALL validate it using sys.sp_describe_first_result_set before execution

### Requirement 5

**User Story:** As a user, I want the system to ask clarifying questions when my query is ambiguous, so that I get accurate results.

#### Acceptance Criteria

1. WHEN the generated SQL is invalid or underspecified THEN the system SHALL ask clarifying follow-up questions in the same language (English/Hinglish) as the user's input
2. WHEN a query lacks specific dates THEN the system SHALL ask "Which time period are you interested in?" or "Kaunsa time period chahiye?"
3. WHEN a query references ambiguous entities THEN the system SHALL ask for clarification (e.g., "Which customer do you mean?" or "Kaunsa customer?")
4. WHEN the user provides clarification THEN the system SHALL remember the context for the current session
5. WHEN the session continues THEN the system SHALL use previous context to improve subsequent query generation

### Requirement 6

**User Story:** As a user, I want to view query results in a convenient format and export them, so that I can analyze and share the data.

#### Acceptance Criteria

1. WHEN a query executes successfully THEN the system SHALL display results in a DataGridView with auto-sizing columns
2. WHEN results are displayed THEN the user SHALL be able to copy selected data to clipboard
3. WHEN the user wants to export data THEN the system SHALL provide CSV export functionality
4. WHEN large result sets are returned THEN the system SHALL handle them efficiently without freezing the UI
5. WHEN no results are found THEN the system SHALL display an appropriate "No data found" message

### Requirement 7

**User Story:** As a user, I want an intuitive interface for asking questions and viewing responses, so that I can interact naturally with the system.

#### Acceptance Criteria

1. WHEN using the application THEN the user SHALL see a read-only RichTextBox displaying the conversation history
2. WHEN entering questions THEN the user SHALL use a TextBox that accepts Enter key to send queries
3. WHEN ready to submit THEN the user SHALL be able to click an "Ask" button as an alternative to Enter
4. WHEN viewing the interface THEN the user SHALL see a visible "SELECT-only enforced" banner indicating read-only mode
5. WHEN checking system status THEN the user SHALL see a status bar showing connection status and query execution progress

### Requirement 8

**User Story:** As a system administrator, I want to configure database connections and LLM settings, so that I can customize the application for my environment.

#### Acceptance Criteria

1. WHEN accessing settings THEN the user SHALL open a Settings dialog for configuration
2. WHEN configuring database THEN the user SHALL be able to set the SQL Server connection string
3. WHEN configuring LLM THEN the user SHALL be able to select provider (OpenAI/Gemini/Ollama) and enter API keys
4. WHEN setting limits THEN the user SHALL be able to configure MaxRows for query results
5. WHEN settings are changed THEN the system SHALL persist them to appsettings.json file

### Requirement 9

**User Story:** As a system administrator, I want comprehensive logging of system operations, so that I can monitor performance and troubleshoot issues.

#### Acceptance Criteria

1. WHEN any LLM prompt is sent THEN the system SHALL log the prompt content and timestamp
2. WHEN SQL is generated THEN the system SHALL log the generated SQL statement
3. WHEN queries execute THEN the system SHALL log execution latency and row counts
4. WHEN errors occur THEN the system SHALL log error details with stack traces
5. WHEN the application runs THEN all logs SHALL be written to structured log files with appropriate log levels

### Requirement 10

**User Story:** As a user, I want the application to use efficient data access patterns, so that queries execute quickly and reliably.

#### Acceptance Criteria

1. WHEN executing SQL queries THEN the system SHALL use Dapper ORM for efficient data mapping
2. WHEN connecting to SQL Server THEN the system SHALL use Microsoft.Data.SqlClient for optimal performance
3. WHEN returning results THEN the system SHALL convert query results to DataTable format for DataGridView binding
4. WHEN managing connections THEN the system SHALL properly dispose of database connections and commands
5. WHEN handling parameters THEN the system SHALL use parameterized queries to prevent SQL injection