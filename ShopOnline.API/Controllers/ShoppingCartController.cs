﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopOnline.API.Extensions;
using ShopOnline.API.Repositories.Contracts;
using ShopOnline.Models.Dtos;

namespace ShopOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly IProductRepository productRepository;

        public ShoppingCartController(IShoppingCartRepository shoppingCartRepository, IProductRepository productRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository;
            this.productRepository = productRepository;
        }
        [HttpGet]
        [Route("{userId}/GetItems")]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetItems(int userId)
        {
            try
            {
                var cartItems = await shoppingCartRepository.GetItems(userId);
                if (cartItems == null)
                {
                    return NoContent();
                }
                var products = await productRepository.GetItems();
                if(products == null)
                {
                    throw new Exception("No products in the system");
                }

                var cartItemDto = cartItems.ConvertToDto(products);
                return Ok(cartItemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CartItemDto>> GetItem(int id)
        {
            try
            {
                var cartItem = await shoppingCartRepository.GetItem(id);
                if(cartItem == null)
                {
                    return NotFound();
                }
                var product = await productRepository.GetItem(cartItem.ProductId);

                if(product == null)
                {
                    return NotFound();
                }
                var cartItemDto = cartItem.ConvertToDto(product);
                return Ok(cartItemDto);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CartItemDto>> PostItem([FromBody] CartItemToAddDto cartItemToAddDto)
        {
            try
            {
                var newCartItem = await shoppingCartRepository.AddItem(cartItemToAddDto);
                if(newCartItem == null)
                {
                    return NoContent();
                }
                
                var product = await productRepository.GetItem(newCartItem.ProductId);

                if(product == null)
                {
                    throw new Exception($"Something went wrong when attemptiong to retrieve product Controller Postitem(productId:({cartItemToAddDto.ProductId})");
                }

                var newCartItemDto = newCartItem.ConvertToDto(product);
                return CreatedAtAction(nameof(GetItem), new { id = newCartItemDto.Id }, newCartItemDto);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }    
        }
    }
}
