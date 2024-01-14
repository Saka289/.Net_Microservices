using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using Web.MessageBus;
using Web.Services.ShoppingCartAPI.Data;
using Web.Services.ShoppingCartAPI.Models;
using Web.Services.ShoppingCartAPI.Models.Dto;
using Web.Services.ShoppingCartAPI.RabbitMQSender;
using Web.Services.ShoppingCartAPI.Service.IService;

namespace Web.Services.ShoppingCartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _response;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IRabbitMQCartMessageSender _messageBus;
        private readonly IConfiguration _configuration;

        public CartAPIController(IMapper mapper, AppDbContext context, IProductService productService, ICouponService couponService, IRabbitMQCartMessageSender messageBus, IConfiguration configuration)
        {
            _context = context;
            this._response = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(await _context.CartHeaders.FirstAsync(u => u.UserId == userId)),
                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_context.CartDetails.Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));
                IEnumerable<ProductDto> productDtos = await _productService.GetProducts();
                foreach (var item in cart.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }
                //apply coupon if any
                if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                    CouponDto coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                    if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;
                    }
                }
                _response.Result = cart;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    await _context.CartHeaders.AddAsync(cartHeader);
                    await _context.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    await _context.CartDetails.AddAsync(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _context.SaveChangesAsync();
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(u => u.ProductId == cartDto.CartDetails.First().ProductId && u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        //create cartdetails
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        await _context.CartDetails.AddAsync(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        //update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _context.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _context.SaveChangesAsync();
                    }
                }
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }


        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = await _context.CartDetails.FirstAsync(u => u.CartDetailsId == cartDetailsId);
                int totalCountofCartItem = _context.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
                _context.CartDetails.Remove(cartDetails);
                if (totalCountofCartItem == 1)
                {
                    var cartHeaderToRemove = await _context.CartHeaders.FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                    _context.CartHeaders.Remove(cartHeaderToRemove);
                }
                await _context.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("UpdateCart")]
        public async Task<ResponseDto> UpdateCart([FromBody] string userId)
        {
            try
            {
                CartHeader cartHeader = _context.CartHeaders.First(u => u.UserId == userId);
                if (cartHeader != null)
                {
                    var cartDetails = await _context.CartDetails.Where(u => u.CartHeaderId == cartHeader.CartHeaderId).ToListAsync();
                    if (cartHeader != null)
                    {
                        _context.CartDetails.RemoveRange(cartDetails);
                    }
                    _context.CartHeaders.Remove(cartHeader);
                }
                await _context.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _context.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _context.CartHeaders.Update(cartFromDb);
                await _context.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _context.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = string.Empty;
                _context.CartHeaders.Update(cartFromDb);
                await _context.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                _messageBus.SendMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }
    }
}