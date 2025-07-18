using Xunit;
using Moq;
using WebManager.Controllers;
using WebManager.Models;
using WebManager.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace WebManager.Backend.Tests
{
    public class FinancesControllerTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly FinancesController _controller;

        public FinancesControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _mockContext = new Mock<AppDbContext>(options);
            _controller = new FinancesController(_mockContext.Object);
        }

        private void SetUserContext(int userId, string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsFinancesForAuthorizedUser()
        {
            // Arrange
            SetUserContext(1, "Test User");
            var finances = new List<Finance>
            {
                new Finance { Id = 1, UserId = 1, Description = "Test Finance 1", Amount = 100, Type = "income" },
                new Finance { Id = 2, UserId = 1, Description = "Test Finance 2", Amount = 50, Type = "expense" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Finance>>();
            mockSet.As<IQueryable<Finance>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Finance>(finances.Provider));
            mockSet.As<IQueryable<Finance>>().Setup(m => m.Expression).Returns(finances.Expression);
            mockSet.As<IQueryable<Finance>>().Setup(m => m.ElementType).Returns(finances.ElementType);
            mockSet.As<IQueryable<Finance>>().Setup(m => m.GetEnumerator()).Returns(finances.GetEnumerator());

            _mockContext.Setup(c => c.Finances).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedFinances = Assert.IsType<List<Finance>>(okResult.Value);
            Assert.Equal(2, returnedFinances.Count);
            Assert.Contains(returnedFinances, f => f.Description == "Test Finance 1");
            Assert.Contains(returnedFinances, f => f.Description == "Test Finance 2");
        }

        // Helper classes for IAsyncEnumerable and IAsyncQueryProvider mocking
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var enumerable = _inner.Execute<IEnumerable<TEntity>>(expression);
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new[] { enumerable.Cast<object>().ToList() });
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }

        [Fact]
        public async Task GetAll_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange - No user context set
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }
    }
}