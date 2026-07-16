using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Dtos;
using Core.Entities;
using Data.Context;
using BCrypt.Net;
using System.Runtime;

namespace Business.Services
{
    public class AuthService
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;

        public AuthService(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Kayit Olma islemi
        public bool Register(RegisterDto Dto)
        {
            // Bu email adresiyle önceden kayıt oluşturulmuş mu ?
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Dto.Email || u.UserName == Dto.UserName );
            if (existingUser != null)
            {
                return false; // kullanıcı var
            }

            // Şifre Hashleme işlemi
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(Dto.Password);

            // Yeni kullanıcı oluştur
            var user = new AppUser
            { 
                UserName = Dto.UserName,
                Email = Dto.Email,
                PasswordHash = passwordHash,
                CreateAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return true;
        }

        // Giriş Yapma (Hash dogrulancak)
        public AppUser? Login(LoginDto dto)
        {
            // 1. Önce email ile kullanıcıyı veritabanında bul
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
            {
                return null; // 
            }

            // 2. Girilen ham şifre ile veritabanındaki hash'i karşılaştır
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null; // 
            }

            return user; 
        }
        public List<UserSummaryDto> GetAllUsers()
        {
            return _context.Users
                .ProjectTo<UserSummaryDto>(_mapper.ConfigurationProvider)
                .ToList();
        }
    }
}
