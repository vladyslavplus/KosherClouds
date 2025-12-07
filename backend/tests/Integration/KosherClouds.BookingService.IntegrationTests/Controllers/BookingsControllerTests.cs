using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.IntegrationTests.Infrastructure;

namespace KosherClouds.BookingService.IntegrationTests.Controllers
{
    public class BookingsControllerTests : IClassFixture<BookingServiceWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public BookingsControllerTests(BookingServiceWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetBookings_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/bookings");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetBookings_AsUser_ShouldReturnOnlyOwnBookings()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            await CreateBooking(userId);

            var response = await _client.GetAsync("/api/bookings");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<BookingResponseDto>>(JsonOptions);
            bookings.Should().NotBeNull();
            bookings!.Should().HaveCountGreaterOrEqualTo(1);
            bookings!.All(b => b.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetBookings_AsAdmin_ShouldReturnAllBookings()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            await CreateBooking(user1);
            await CreateBooking(user2);

            var response = await _client.GetAsync("/api/bookings");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<BookingResponseDto>>(JsonOptions);
            bookings.Should().NotBeNull();
            bookings!.Should().HaveCountGreaterOrEqualTo(2);
        }

        [Fact]
        public async Task GetBookingById_WithValidId_ShouldReturnBooking()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var bookingId = await CreateBooking(userId);

            var response = await _client.GetAsync($"/api/bookings/{bookingId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
            booking.Should().NotBeNull();
            booking!.Id.Should().Be(bookingId);
            booking.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetBookingById_ByDifferentUser_ShouldReturnNotFound()
        {
            var originalUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _client.AddAuthorizationHeader(originalUserId);
            var bookingId = await CreateBooking(originalUserId);

            _client.AddAuthorizationHeader(differentUserId);

            var response = await _client.GetAsync($"/api/bookings/{bookingId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBooking_WithValidData_ShouldReturnCreated()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var counter = Interlocked.Increment(ref _bookingCounter);

            var dto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(5 + (counter / 24)).AddHours(12 + (counter % 10)),
                Adults = 2,
                Children = 1,
                Zone = "MainHall",
                PhoneNumber = "+380123456789",
                Comment = "Test booking"
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
            booking.Should().NotBeNull();
            booking!.Adults.Should().Be(2);
            booking.Children.Should().Be(1);
            booking.Zone.Should().Be("MainHall");
        }

        [Fact]
        public async Task CreateBooking_WithPastDate_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var dto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(-1),
                Adults = 2,
                Children = 0,
                Zone = "MainHall",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateBooking_OutsideWorkingHours_ShouldReturnConflict()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var dto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
                Adults = 2,
                Children = 0,
                Zone = "MainHall",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateBooking_TooManyGuests_ShouldReturnConflict()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var dto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(1).AddHours(12),
                Adults = 40,
                Children = 15,
                Zone = "MainHall",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateBooking_WithHookahs_ShouldIncludeHookahs()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var counter = Interlocked.Increment(ref _bookingCounter);

            var dto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(10 + (counter / 24)).AddHours(14 + (counter % 8)),
                Adults = 3,
                Children = 0,
                Zone = "VIP",
                PhoneNumber = "+380123456789",
                Hookahs = new List<HookahBookingDto>
                {
                    new()
                    {
                        TobaccoFlavor = "Apple",
                        TobaccoFlavorUk = "Яблуко",
                        Strength = "Medium",
                        PriceSnapshot = 300m
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
            booking.Should().NotBeNull();
            booking!.Hookahs.Should().HaveCount(1);
            booking.Hookahs[0].TobaccoFlavor.Should().Be("Apple");
        }

        [Fact]
        public async Task UpdateBooking_AsAdmin_ShouldUpdateSuccessfully()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var bookingId = await CreateBooking(userId);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var updateDto = new BookingUpdateDto
            {
                Adults = 5,
                Comment = "Updated by admin"
            };

            var response = await _client.PutAsJsonAsync($"/api/bookings/{bookingId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateBooking_AsRegularUser_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var bookingId = await CreateBooking(userId);

            var updateDto = new BookingUpdateDto
            {
                Adults = 5,
                Comment = "Trying to update"
            };

            var response = await _client.PutAsJsonAsync($"/api/bookings/{bookingId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CancelBooking_AsOwner_ShouldCancelSuccessfully()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var bookingId = await CreateBooking(userId);

            var response = await _client.PostAsync($"/api/bookings/{bookingId}/cancel", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/bookings/{bookingId}");
            var booking = await getResponse.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
            booking!.Status.Should().Be("Cancelled");
        }

        [Fact]
        public async Task CancelBooking_ByDifferentUser_ShouldReturnNotFound()
        {
            var originalUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _client.AddAuthorizationHeader(originalUserId);
            var bookingId = await CreateBooking(originalUserId);

            _client.AddAuthorizationHeader(differentUserId);

            var response = await _client.PostAsync($"/api/bookings/{bookingId}/cancel", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteBooking_AsAdmin_ShouldDeleteSuccessfully()
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _client.AddAuthorizationHeader(userId);
            var bookingId = await CreateBooking(userId);

            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            await _client.PostAsync($"/api/bookings/{bookingId}/cancel", null);

            var response = await _client.DeleteAsync($"/api/bookings/{bookingId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _client.AddAuthorizationHeader(userId);
            var getResponse = await _client.GetAsync($"/api/bookings/{bookingId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteBooking_AsRegularUser_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId);

            var bookingId = await CreateBooking(userId);

            var response = await _client.DeleteAsync($"/api/bookings/{bookingId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static int _bookingCounter = 0;

        private async Task<Guid> CreateBooking(Guid userId)
        {
            var originalAuth = _client.DefaultRequestHeaders.Authorization;
            _client.AddAuthorizationHeader(userId);

            var counter = Interlocked.Increment(ref _bookingCounter);

            int slotsPerDay = 10;
            int dayOffset = counter / slotsPerDay;
            int hourOffset = counter % slotsPerDay;

            var bookingDto = new BookingCreateDto
            {
                BookingDateTime = DateTime.UtcNow.AddDays(2 + dayOffset).Date.AddHours(12 + hourOffset),
                Adults = 2,
                Children = 0,
                Zone = "MainHall",
                PhoneNumber = "+380123456789",
                Comment = "Test booking"
            };

            var response = await _client.PostAsJsonAsync("/api/bookings", bookingDto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"CreateBooking failed via API. Status: {response.StatusCode}. Body: {errorContent}");
            }

            var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);

            if (originalAuth != null)
            {
                _client.DefaultRequestHeaders.Authorization = originalAuth;
            }

            return booking!.Id;
        }
    }
}