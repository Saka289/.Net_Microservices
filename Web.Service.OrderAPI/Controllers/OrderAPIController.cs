using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.MessageBus;
using Web.Service.OrderAPI.Models;
using Web.Service.OrderAPI.Models.Dto;
using Web.Service.OrderAPI.Utility;
using Web.Services.OrderAPI.Data;
using Web.Services.OrderAPI.Service.IService;

namespace Web.Service.OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IProductService _productService;

        public OrderAPIController(IMapper mapper, AppDbContext context, IProductService productService)
        {
            _context = context;
            this._response = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

                OrderHeader orderCreated = _context.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _context.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
