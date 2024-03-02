using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.QueryableExtensions;
using LinqToDB;
using LinqToDB.Mapping;
using System.Linq.Expressions;

namespace RosPitukhNadzor
{
    [Table("warnings")]
    internal class WarningTable
    {
        [Column("chat_id"), NotNull, DataType("INTEGER")]
        public long ChatID;
        [Column("from_user_id"), NotNull, DataType("INTEGER")]
        public long FromUserID;
        [Column("to_user_id_id"), NotNull, DataType("INTEGER")]
        public long ToUserID;
        [Column("warning_expiried"), NotNull, DataType("INTEGER")]
        public long WarningExpiried;
    }

    [Table("banwords")]
    internal class BanWordTable
    {
        [Column("chat_id"), NotNull, DataType("INTEGER")]
        public long ChatID;
        [Column("word"), NotNull, DataType("TEXT")]
        public string Word;
    }

    internal class DatabaseStorageProvider : IStorageProvider
    {
        private ConfigurationDatabase config;
        private DataContext context;

        private static readonly MapperConfiguration mapper_config;
        private static readonly IMapper mapper;

        static DatabaseStorageProvider()
        {
            mapper_config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Warning, WarningTable>()
                    .ForMember(x => x.WarningExpiried, opt => opt.MapFrom(src => new DateTimeOffset(src.WarningExpiried).ToUnixTimeSeconds()));
                cfg.CreateMap<WarningTable, Warning>()
                    .ForMember(x => x.WarningExpiried, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.WarningExpiried).LocalDateTime));

                cfg.CreateMap<BanWord, BanWordTable>()
                    .ReverseMap();

            });
            mapper = mapper_config.CreateMapper();
        }
        public DatabaseStorageProvider(IConfigurationProvider configurationProvider)
        {
            config = configurationProvider.GetConfiguration(new ConfigurationDatabaseValidator());
            context = new DataContext(config.DatabaseProvider!, config.DatabaseConfig!);

            context.CreateTable<WarningTable>(tableOptions: TableOptions.CreateIfNotExists);
            context.CreateTable<BanWordTable>(tableOptions: TableOptions.CreateIfNotExists);
        }

        public async Task AddWarningAsync(Warning warning)
        {
            await context.InsertAsync(mapper.Map<WarningTable>(warning));
        }
        public async Task<int> RemoveWarningAsync(Expression<Func<Warning, bool>> expression)
        {
            return await context.GetTable<WarningTable>().
                DeleteAsync(mapper.MapExpression<Expression<Func<WarningTable, bool>>>(expression));
        }

        public async Task<int> ClearExpiredWarningsAsync()
        {
            return await context.GetTable<WarningTable>().
                DeleteAsync(d => d.WarningExpiried < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
        }

        public IEnumerable<Warning> GetWarnings(Expression<Func<Warning, bool>> expression)
        {
            var tables = context.GetTable<WarningTable>()
                .Where(mapper.MapExpression<Expression<Func<WarningTable, bool>>>(expression));

            return tables.ProjectTo<Warning>(mapper_config).AsEnumerable();
        }

        public async Task AddBanWordAsync(BanWord word)
        {
            await context.InsertAsync(mapper.Map<BanWordTable>(word));
        }

        public async Task<int> RemoveBanWordAsync(Expression<Func<BanWord, bool>> expression)
        {
            return await context.GetTable<BanWordTable>().
                DeleteAsync(mapper.MapExpression<Expression<Func<BanWordTable, bool>>>(expression));
        }

        public IEnumerable<BanWord> GetBanWords(Expression<Func<BanWord, bool>> expression)
        {
            var tables = context.GetTable<BanWordTable>()
                .Where(mapper.MapExpression<Expression<Func<BanWordTable, bool>>>(expression));

            return tables.ProjectTo<BanWord>(mapper_config).AsEnumerable();
        }
    }
}
