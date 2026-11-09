using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace EzCondo.BackEndServer.Data.Entities.SchemaIdentity
{
	public class User : IdentityUser<Guid>, IAuditableEntity, ISoftDeletableEntity
	{ //Tenant trả lương
		public string FullName { get; set; } = string.Empty;

		//Nhân viên/Quản lý
		public string? AvatarUrl { get; set; }
		public UserStatusEnum Status { get; set; } = UserStatusEnum.Inviting;
		public string? CitizenId { get; set; } //cccd
		public string? EmployeementContract { get; set; } //Hợp đồng lao động
		public DateOnly? DateOfBirth { get; set; }
		public string? BankName { get; set; }
		public string? BankAccountNumber { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid? TenantId { get; set; }
		public Tenant? Tenant { get; set; }

		public ICollection<Rule> Rules { get; set; } = new List<Rule>();
		public ICollection<Complaint> ComplaintResident { get; set; } = new List<Complaint>();
		public ICollection<Complaint> ComplaintStaff { get; set; } = new List<Complaint>();
		public ICollection<Invoice> CreatedByStaff { get; set; } = new List<Invoice>();
		public ICollection<Invoice> UpdatedByStaff { get; set; } = new List<Invoice>();
		public ICollection<Invoice> DeletedByStaff { get; set; } = new List<Invoice>();
		public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
		public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
		public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
		public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
		public ICollection<SubscriptionPlan> Created { get; set; } = new List<SubscriptionPlan>();
		public ICollection<SubscriptionPlan> Updated { get; set; } = new List<SubscriptionPlan>();
		public ICollection<SubscriptionPlan> Deleted { get; set; } = new List<SubscriptionPlan>();
		public ICollection<Invitation> CreatedByUser { get; set; } = new List<Invitation>();
		public ICollection<Invitation> TargetUser { get; set; } = new List<Invitation>();
		public ICollection<Service> CreatedByManager { get; set; } = new List<Service>();
		public ICollection<Service> UpdatedByManager { get; set; } = new List<Service>();
		public ICollection<Service> DeletedByManager { get; set; } = new List<Service>();
		public ICollection<StaffTimeSheet> ManagerCreated { get; set; } = new List<StaffTimeSheet>();
		public ICollection<StaffTimeSheet> Staff { get; set; } = new List<StaffTimeSheet>();
		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
		public ICollection<UserRole> GrantedUserRoles { get; set; } = new List<UserRole>();
		public ICollection<InvoicePaymentAttempt> InvoicePaymentAttempts { get; set; } = new List<InvoicePaymentAttempt>();
		public ICollection<PayrollDisbursement> ApprovedBy { get; set; } = new List<PayrollDisbursement>();
		public ICollection<PayrollDisbursement> IssuedBy { get; set; } = new List<PayrollDisbursement>();
		public ICollection<PayrollDisbursementItem> PayrollDisbursementItems { get; set; } = new List<PayrollDisbursementItem>();
		public ICollection<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; set; } = new List<SubscriptionPaymentAttempt>();

	}
}
