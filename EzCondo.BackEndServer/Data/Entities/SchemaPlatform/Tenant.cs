using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public class Tenant
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Slug { get; set; }
		public string LegalName { get; set; }
		public string TaxCode { get; set; }
		public string Address { get; set; }

		public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
		public ICollection<Rule> Rules{ get; set; } = new List<Rule>();
		public ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();
		public ICollection<PaymentProviderConfig> PaymentProviderConfigs { get; set; } = new List<PaymentProviderConfig>();
		public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
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
	}
}
