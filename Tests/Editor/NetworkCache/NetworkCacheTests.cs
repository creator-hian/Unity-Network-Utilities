using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Hian.NetworkUtilities;

/// <summary>
/// NetworkCache 클래스의 기능을 테스트합니다.
/// </summary>
/// <remarks>
/// 테스트 대상:
/// - 기본 CRUD 작업
/// - 캐시 만료
/// - 이벤트 발생
/// - LRU 캐시 교체 정책
/// - 동시성 처리
/// </remarks>
[TestFixture]
public class NetworkCacheTests
{
    private NetworkCache _cache;

    /// <summary>
    /// 각 테스트 실행 전에 새로운 NetworkCache 인스턴스를 생성합니다.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _cache = new NetworkCache(maxCacheSize: 5);
    }

    /// <summary>
    /// 각 테스트 실행 후 리소스를 정리합니다.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
    }

    #region Basic CRUD Tests
    /// <summary>
    /// 캐시 데이터 저장 및 조회 기능을 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 데이터가 성공적으로 저장되는지 확인
    /// - 저장된 데이터를 정확히 조회할 수 있는지 확인
    /// </remarks>
    [Test]
    public void SetAndGetCache_WithValidData_WorksCorrectly()
    {
        // Arrange
        var testData = "test data";
        
        // Act
        _cache.SetCache("test_key", testData);
        var success = _cache.TryGetCache<string>("test_key", out var result);

        // Assert
        Assert.That(success, Is.True, "Cache retrieval should be successful");
        Assert.That(result, Is.EqualTo(testData), "Retrieved data should match stored data");
    }

    /// <summary>
    /// 캐시 항목 제거 기능을 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 기존 항목이 성공적으로 제거되는지 확인
    /// - 제거된 항목이 더 이상 조회되지 않는지 확인
    /// </remarks>
    [Test]
    public void RemoveCache_ExistingItem_RemovesSuccessfully()
    {
        // Arrange
        _cache.SetCache("test_key", "test data");

        // Act
        var removed = _cache.RemoveCache("test_key");
        var exists = _cache.TryGetCache<string>("test_key", out _);

        // Assert
        Assert.That(removed, Is.True, "Cache removal should be successful");
        Assert.That(exists, Is.False, "Removed item should not be retrievable");
    }
    #endregion

    #region Expiration Tests
    /// <summary>
    /// 캐시 만료 기능을 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 만료 시간이 지난 항목이 조회되지 않는지 확인
    /// - 만료 처리가 자동으로 이루어지는지 확인
    /// </remarks>
    [Test]
    public async Task ExpiredCache_IsNotRetrievable()
    {
        // Arrange
        _cache.SetCache("test_key", "data", expirationMinutes: 0.016f); // 1초
        
        // Act
        await Task.Delay(1100); // 1.1초 대기
        var success = _cache.TryGetCache<string>("test_key", out _);
        
        // Assert
        Assert.That(success, Is.False);
    }
    #endregion

    #region Event Tests
    /// <summary>
    /// 캐시 이벤트 발생을 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 항목 추가 시 OnCacheAdded 이벤트 발생 확인
    /// - 항목 제거 시 OnCacheRemoved 이벤트 발생 확인
    /// </remarks>
    [Test]
    public void CacheEvents_AreTriggeredCorrectly()
    {
        // Arrange
        var addedTriggered = false;
        var removedTriggered = false;
        
        _cache.OnCacheAdded += _ => addedTriggered = true;
        _cache.OnCacheRemoved += _ => removedTriggered = true;

        // Act
        _cache.SetCache("test", "data");
        _cache.RemoveCache("test");

        // Assert
        Assert.That(addedTriggered, Is.True, "OnCacheAdded event should be triggered");
        Assert.That(removedTriggered, Is.True, "OnCacheRemoved event should be triggered");
    }
    #endregion

    #region LRU Tests
    /// <summary>
    /// LRU(Least Recently Used) 캐시 교체 정책을 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 캐시가 가득 찼을 때 가장 오래된 항목이 제거되는지 확인
    /// - 최대 캐시 크기가 유지되는지 확인
    /// </remarks>
    [Test]
    public void LRUCache_EvictsOldestItem_WhenFull()
    {
        // Arrange & Act
        for (int i = 0; i < 6; i++) // maxSize + 1
        {
            _cache.SetCache($"key_{i}", $"value_{i}");
        }

        // Assert
        var hasFirst = _cache.TryGetCache<string>("key_0", out _);
        Assert.That(hasFirst, Is.False, "Oldest item should have been evicted");
    }
    #endregion

    #region Concurrent Access Tests
    /// <summary>
    /// 동시성 처리를 테스트합니다.
    /// </summary>
    /// <remarks>
    /// 검증 항목:
    /// - 여러 스레드에서 동시에 접근해도 안전하게 동작하는지 확인
    /// - 캐시 크기 제한이 동시 접근 상황에서도 유지되는지 확인
    /// </remarks>
    [Test]
    public async Task ConcurrentAccess_IsThreadSafe()
    {
        // Arrange
        var tasks = new Task[100];
        var maxCacheSize = _cache.GetStats().MaxCacheSize;
        var random = new Random();
        
        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            var index = i;
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(random.Next(1, 10));
                    var key = $"key_{index % maxCacheSize}";
                    _cache.SetCache(key, $"value_{index}");
                    _cache.TryGetCache<string>(key, out _);
                }
                catch (InvalidOperationException)
                {
                    // 재시도 실패는 예상된 동작이므로 무시
                }
            });
        }

        // Assert
        await Task.WhenAll(tasks);
        var stats = _cache.GetStats();
        
        Assert.That(stats.TotalItems, Is.LessThanOrEqualTo(maxCacheSize), 
            "Cache size should not exceed maximum");
        Assert.That(stats.TotalItems, Is.GreaterThan(0), 
            "Cache should contain items after concurrent operations");
    }
    #endregion
} 