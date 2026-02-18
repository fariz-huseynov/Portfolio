namespace Portfolio.Domain.Constants;

public static class Permissions
{
    public const string DashboardView = "Dashboard.View";

    public const string BlogsView = "Blogs.View";
    public const string BlogsCreate = "Blogs.Create";
    public const string BlogsEdit = "Blogs.Edit";
    public const string BlogsDelete = "Blogs.Delete";

    public const string ProjectsView = "Projects.View";
    public const string ProjectsCreate = "Projects.Create";
    public const string ProjectsEdit = "Projects.Edit";
    public const string ProjectsDelete = "Projects.Delete";

    public const string LeadsView = "Leads.View";
    public const string LeadsMarkRead = "Leads.MarkRead";

    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";
    public const string UsersResetPassword = "Users.ResetPassword";

    public const string SiteContentView = "SiteContent.View";
    public const string SiteContentEdit = "SiteContent.Edit";

    public const string SettingsView = "Settings.View";
    public const string SettingsEdit = "Settings.Edit";

    public const string LogsView = "Logs.View";
    public const string LogsDelete = "Logs.Delete";

    public const string SecurityView = "Security.View";
    public const string SecurityManage = "Security.Manage";

    public const string FilesManage = "Files.Manage";

    public const string AiContentGenerate = "AiContent.Generate";
    public const string AiContentView = "AiContent.View";

    public static IReadOnlyList<string> All =>
    [
        DashboardView,
        BlogsView, BlogsCreate, BlogsEdit, BlogsDelete,
        ProjectsView, ProjectsCreate, ProjectsEdit, ProjectsDelete,
        LeadsView, LeadsMarkRead,
        UsersView, UsersCreate, UsersEdit, UsersDelete, UsersResetPassword,
        SiteContentView, SiteContentEdit,
        SettingsView, SettingsEdit,
        LogsView, LogsDelete,
        SecurityView, SecurityManage,
        FilesManage,
        AiContentGenerate, AiContentView
    ];
}
