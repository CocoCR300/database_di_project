using Microsoft.EntityFrameworkCore;

using Restify.API.Models;

namespace Restify.API.Data
{
	public class RestifyDbContext : DbContext
	{
		public DbSet<Booking> Booking { get; set; }
		public DbSet<Lodging> Lodging { get; set; }
		public DbSet<PhoneNumber> PhoneNumber { get; set; }
		public DbSet<Room> Room { get; set; }
		public DbSet<RoomBooking> RoomBooking { get; set; }
		public DbSet<User> User { get; set; }
		
		public RestifyDbContext() { }
		public RestifyDbContext(DbContextOptions<RestifyDbContext> options) : base(options) { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				string connectionString = "server=localhost;user=root;password=;database=restify";
				optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserRole>(userRole =>
			{
				userRole.HasKey(u => u.Id);

				userRole.Property(u => u.Id)
					.HasColumnName("userRoleId")
					.IsRequired();

				userRole.Property(u => u.Type)
					.IsRequired()
					.HasMaxLength(50);
			});

			modelBuilder.Entity<User>(user =>
			{
				user.HasKey(u => u.Name);

				user.Property(ar => ar.Name)
					.HasColumnName("userName")
					.IsRequired()
					.HasMaxLength(50);

				user.Property(u => u.RoleId)
					.HasColumnName("userRoleId")
					.IsRequired();

				user.Property(ar => ar.Password)
					.IsRequired()
					.HasMaxLength(100);

				user.HasOne(u => u.Role).WithMany().OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<PhoneNumber>(phoneNumber =>
			{
				phoneNumber.HasKey(p => new { p.PersonId, p.Number });

				phoneNumber.Property(p => p.PersonId)
					.IsRequired();

				phoneNumber.Property(p => p.Number)
					.IsRequired()
					.HasColumnType("CHAR(30)")
					.HasColumnName("phoneNumber");
			});

			modelBuilder.Entity<Person>(person =>
			{
				person.HasKey(p => p.Id);

				person.Property(p => p.Id)
					.HasColumnName("personId")
					.IsRequired();

				person.Property(p => p.UserName)
					.HasColumnName("userName")
					.IsRequired()
					.HasMaxLength(50);

				person.Property(p => p.FirstName)
					.IsRequired()
					.HasMaxLength(50);

				person.Property(p => p.LastName)
					.IsRequired()
					.HasMaxLength(100);

				person.Property(p => p.EmailAddress)
					.IsRequired()
					.HasAnnotation("EmailAddress", null)
					.HasMaxLength(150);

				person.HasOne(p => p.User)
					.WithOne(u => u.Person)
					.HasForeignKey<Person>(p => p.UserName);

				person.HasMany(p => p.PhoneNumbers)
					.WithOne()
					.HasForeignKey(p => p.PersonId);
			});

			modelBuilder.Entity<Lodging>(lodging =>
			{
				lodging.HasKey(l => l.Id);

				lodging.Property(l => l.Id)
					.IsRequired();

				lodging.Property(l => l.Id)
					.IsRequired()
					.HasColumnName("LodgingId");

				lodging.Property(l => l.OwnerId)
					.IsRequired()
					.HasColumnName("ownerPersonId");

				lodging.Property(l => l.LodgingType)
					.IsRequired()
					.HasMaxLength(50);

				lodging.Property(l => l.Name)
					.IsRequired()
					.HasMaxLength(100);

				lodging.Property(l => l.Address)
					.IsRequired()
					.HasMaxLength(300);

				lodging.Property(l => l.Description)
					.IsRequired()
					.HasMaxLength(1000);

				lodging.HasMany(l => l.Rooms)
					.WithOne(r => r.Lodging)
					.HasForeignKey(r => r.LodgingId);

				lodging.HasOne(l => l.Owner)
					.WithMany()
					.HasForeignKey(l => l.OwnerId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Booking>(booking =>
			{
				booking.HasKey(b => b.Id);

				booking.Property(b => b.Id)
					.IsRequired()
					.HasColumnName("bookingId");
				
				booking.Property(b => b.CustomerId)
					.IsRequired()
					.HasColumnName("customerPersonId");

				booking.Property(b => b.LodgingId)
					.IsRequired();

				booking.Property(b => b.Status)
					.IsRequired()
					.HasMaxLength(50);

				booking.Property(b => b.StartDate)
					.IsRequired();

				booking.Property(b => b.EndDate)
					.IsRequired();

				booking.HasMany(r => r.RoomBookings)
					.WithOne(r => r.Booking)
					.HasForeignKey(r => r.BookingId);

				booking.HasOne(b => b.Customer)
					.WithMany()
					.HasForeignKey(b => b.CustomerId)
					.OnDelete(DeleteBehavior.Restrict);

				booking.HasOne(b => b.Lodging)
					.WithMany()
					.HasForeignKey(b => b.LodgingId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Payment>(payment =>
			{
				payment.HasKey(p => p.Id);

				payment.Property(p => p.Id)
					.IsRequired()
					.HasColumnName("paymentId");

				payment.Property(p => p.BookingId)
					.IsRequired();

				payment.Property(p => p.DateAndTime)
					.IsRequired();

				payment.HasOne<Booking>()
					.WithOne(b => b.Payment)
					.HasForeignKey<Payment>(p => p.BookingId);
			});

			modelBuilder.Entity<Room>(room =>
			{
				room.HasKey(new string[] { nameof(Models.Room.LodgingId), nameof(Models.Room.Number) });

				room.Property(r => r.LodgingId)
					.IsRequired();

				room.Property(r => r.Number)
					.IsRequired()
					.HasColumnName("roomNumber");

				room.Property(r => r.Occupied)
					.IsRequired();

				room.Property(r => r.PerNightPrice)
					.IsRequired();

				room.Property(r => r.Capacity)
					.IsRequired();
			});

			modelBuilder.Entity<RoomBooking>(roomBooking =>
			{
				roomBooking.HasKey(r => r.Id);

				roomBooking.Property(r => r.Id)
					.IsRequired()
					.HasColumnName("roomBookingId");

				roomBooking.Property(r => r.BookingId)
					.IsRequired();

				roomBooking.Property(r => r.LodgingId)
					.IsRequired();

				roomBooking.Property(r => r.RoomNumber)
					.IsRequired();

				roomBooking.Property(r => r.Cost)
					.IsRequired();

				roomBooking.Property(r => r.Fees)
					.IsRequired();

				roomBooking.HasOne(r => r.Room)
					.WithMany()
					.HasForeignKey(r => new { r.LodgingId, r.RoomNumber });
			});
		}
	}
}
