using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration
	.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("The 'DefaultConnection' connectionString is not found.");

//Cấu hình Data Source cho enums
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
//Bật tính năng Jsonb
dataSourceBuilder.EnableDynamicJson();
//Map enums
dataSourceBuilder.MapEnum<AiRoleEnum>("tenant.ai_role_enum");
dataSourceBuilder.MapEnum<ApartmentStatusEnum>("tenant.apartment_status_enum");
dataSourceBuilder.MapEnum<BillingPeriodStatus>("tenant.billing_period_status");
dataSourceBuilder.MapEnum<DocumentStatusEnum>("tenant.document_status_enum");
dataSourceBuilder.MapEnum<DocumentTypeEnum>("tenant.document_type_enum");
dataSourceBuilder.MapEnum<PaymentOrderStatus>("tenant.payment_order_status");
dataSourceBuilder.MapEnum<PaymentTransactionStatus>("tenant.payment_transaction_status");
dataSourceBuilder.MapEnum<PayrollStatusEnum>("tenant.payroll_status_enum");
dataSourceBuilder.MapEnum<ResidentComplaintStatus>("tenant.resident_complaint_status");
dataSourceBuilder.MapEnum<ServiceBillingType>("tenant.service_billing_type");
dataSourceBuilder.MapEnum<SubscriptionInvoiceStatus>("platform.subscription_invoice_status");
dataSourceBuilder.MapEnum<SubscriptionStatusEnum>("platform.subscription_status_enum");
dataSourceBuilder.MapEnum<SupportTicketStatus>("platform.support_ticket_status");
dataSourceBuilder.MapEnum<SystemIncidentStatus>("platform.system_incident_status");
dataSourceBuilder.MapEnum<TenantContractStatus>("tenant.tenant_contract_status");
dataSourceBuilder.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
dataSourceBuilder.MapEnum<TenantResidentStatus>("tenant.tenant_resident_status");
dataSourceBuilder.MapEnum<TimeSheetStatus>("tenant.time_sheet_status");
dataSourceBuilder.MapEnum<UserStatusEnum>("identity.user_status_enum");
dataSourceBuilder.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseNpgsql(dataSource));

//builder.Services.AddScoped<DbInitializer>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//using (var scope = app.Services.CreateScope())
//{
//	var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
//	//await initializer.SeedAsync();
//}
 
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
