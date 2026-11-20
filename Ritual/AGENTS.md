# Unity workflow

- At the beginning of every chat in this project, connect to Unity's MCP and check the active Unity project before starting other work.
- Use the `unity` MCP server, configured with the installed Unity CLI (`unity.exe mcp`). The old `unity_mcp` relay and `unityMCP` HTTP entries are disabled. Discover and call its `editor_status` tool to check connectivity, and confirm the connected project is `D:\Coding3\Narrative-Coding\Ritual` before changing editor state. Use tool calls for the health check; this server does not advertise MCP resources.
- If the configured Unity MCP connection fails, report the failure and use the available editor or project files to continue the task.
