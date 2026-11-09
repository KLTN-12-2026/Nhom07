using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public class Tenant : IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Slug { get; set; } = null!;
		public string LegalName { get; set; } = null!;
		public string TaxCode { get; set; } = null!;
		public string Address { get; set; } = null!;
		public TenantStatusEnum Status { get; set; } = TenantStatusEnum.Trial;
		public DateTimeOffset? TrialEndsAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
		public ICollection<Rule> Rules{ get; set; } = new List<Rule>();
		public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
		public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
		public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
		public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
		public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
		public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
		public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
		public ICollection<Role> Roles { get; set; } = new List<Role>();
		public ICollection<Service> Services { get; set; } = new List<Service>();
		public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<ProviderAccount> ProviderAccounts { get; set; } = new List<ProviderAccount>();
		public ICollection<SubscriptionInvoice> SubscriptionInvoices { get; set; } = new List<SubscriptionInvoice>();
		public ICollection<InvoicePaymentAttempt> InvoicePaymentAttempts { get; set; } = new List<InvoicePaymentAttempt>();
		public ICollection<InvoicePaymentAllocation> InvoicePaymentAllocations { get; set; } = new List<InvoicePaymentAllocation>();
		public ICollection<PayrollDisbursement> PayrollDisbursements { get; set; } = new List<PayrollDisbursement>();
		public ICollection<PayrollDisbursementItem> PayrollDisbursementItems { get; set; } = new List<PayrollDisbursementItem>();
	}
}
