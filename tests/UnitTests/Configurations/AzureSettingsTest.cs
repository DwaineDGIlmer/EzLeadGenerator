namespace Application.Configurations.Tests
{
    public class AzureSettingsTest
    {
        [Fact]
        public void AzureTableName_ShouldHaveDefaultValue()
        {
            // Arrange
            var settings = new AzureSettings();

            // Act
            var tableName = settings.AzureTableName;

            // Assert
            Assert.Equal(Application.Constants.Defaults.AzureTableName, tableName);
        }

        [Fact]
        public void AzureTableName_CanBeSet()
        {
            // Arrange
            var settings = new AzureSettings();
            var expectedTableName = "CustomTableName";

            // Act
            settings.AzureTableName = expectedTableName;

            // Assert
            Assert.Equal(expectedTableName, settings.AzureTableName);
        }

        [Fact]
        public void CompanyProfileTableName_ShouldHaveDefaultValue()
        {
            // Arrange
            var settings = new AzureSettings();

            // Act
            var tableName = settings.CompanyProfilePartionKey;

            // Assert
            Assert.Equal(Application.Constants.Defaults.CompanyProfilePartionKey, tableName);
        }

        [Fact]
        public void CompanyProfileTableName_CanBeSet()
        {
            // Arrange
            var settings = new AzureSettings();
            var expectedTableName = "CustomCompanyProfileTable";

            // Act
            settings.CompanyProfilePartionKey = expectedTableName;

            // Assert
            Assert.Equal(expectedTableName, settings.CompanyProfilePartionKey);
        }

        [Fact]
        public void JobSummaryTableName_ShouldHaveDefaultValue()
        {
            // Arrange
            var settings = new AzureSettings();

            // Act
            var tableName = settings.JobSummaryPartionKey;

            // Assert
            Assert.Equal(Application.Constants.Defaults.AzureTableName, tableName);
        }

        [Fact]
        public void JobSummaryTableName_CanBeSet()
        {
            // Arrange
            var settings = new AzureSettings();
            var expectedTableName = "CustomJobSummaryTable";

            // Act
            settings.JobSummaryPartionKey = expectedTableName;

            // Assert
            Assert.Equal(expectedTableName, settings.JobSummaryPartionKey);
        }
    }
}