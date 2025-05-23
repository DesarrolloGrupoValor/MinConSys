﻿using Dapper;
using MinConSys.Core.Interfaces.Repository;
using MinConSys.Core.Models;
using MinConSys.Core.Models.Base;
using MinConSys.Core.Models.Dto;
using MinConSys.Core.Models.Response;
using MinConSys.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinConSys.Infrastructure.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        protected readonly ConnectionFactory _connectionFactory;
        public MenuRepository(ConnectionFactory connectionFactory) 
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<MenuDto>> ObtenerMenuPorUsuario(string nombreUsuario)
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                var menu =  await connection.QueryAsync<MenuDto>(
                       "usp_ObtenerMenuPorUsuario",
                       new { nombreUsuario },
                       commandType: CommandType.StoredProcedure
                        );

                return menu.ToList();
            }
        }
        public async Task<List<Menu>> GetAllMenusAsync()
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                string sql = @"SELECT 
                    IdMenu,
                    Nombre,
                    NombreInterno,
                    PadreId,
                    Orden,
                    Estado,
                    UsuarioCreacion,
                    FechaCreacion,
                    UsuarioModificacion,
                    FechaModificacion
                FROM Menu
                WHERE Estado = 'A'
                ORDER BY Orden";

                var menus = await connection.QueryAsync<Menu>(sql);
                return menus.ToList();
            }
        }

        public async Task<Menu> GetMenuByIdAsync(int id)
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                string sql = @"SELECT 
                IdMenu,
                Nombre,
                NombreInterno,
                PadreId,
                Orden,
                Estado,
                UsuarioCreacion,
                FechaCreacion,
                UsuarioModificacion,
                FechaModificacion
            FROM Menu
            WHERE IdMenu = @Id AND Estado = 'A'";

                return await connection.QueryFirstOrDefaultAsync<Menu>(sql, new { Id = id });
            }
        }

        public async Task<int> AddMenuAsync(Menu menu)
        {
            using (var connection = await _connectionFactory.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string sql = @"INSERT INTO Menu (
                    Nombre,
                    NombreInterno,
                    PadreId,
                    Orden,
                    Estado,
                    UsuarioCreacion,
                    FechaCreacion
                ) VALUES (
                    @Nombre,
                    @NombreInterno,
                    @PadreId,
                    @Orden,
                    @Estado,
                    @UsuarioCreacion,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = await connection.QuerySingleAsync<int>(sql, menu, transaction);
                    transaction.Commit();
                    return id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<bool> UpdateMenuAsync(Menu menu)
        {
            using (var connection = await _connectionFactory.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string sql = @"UPDATE Menu SET
                    Nombre = @Nombre,
                    NombreInterno = @NombreInterno,
                    PadreId = @PadreId,
                    Orden = @Orden,
                    UsuarioModificacion = @UsuarioModificacion,
                    FechaModificacion = GETDATE()
                WHERE IdMenu = @IdMenu AND Estado = 'A'";

                    var affectedRows = await connection.ExecuteAsync(sql, menu, transaction);
                    transaction.Commit();
                    return affectedRows > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteMenuAsync(int id, string usuario)
        {
            using (var connection = await _connectionFactory.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string sql = @"UPDATE Menu SET
                    Estado = 'I',
                    UsuarioModificacion = @UsuarioModificacion,
                    FechaModificacion = GETDATE()
                WHERE IdMenu = @IdMenu AND Estado = 'A'";

                    var affectedRows = await connection.ExecuteAsync(sql, new
                    {
                        IdMenu = id,
                        UsuarioModificacion = usuario
                    }, transaction);

                    transaction.Commit();
                    return affectedRows > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
