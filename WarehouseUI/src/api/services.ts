import apiClient, { COMPANY_ID } from './apiClient';
import type {
  CreateProductDto,
  UpdateProductDto,
  PagedResponse,
  ProductResponse,
  CreateWarehouseDto,
  UpdateWarehouseDto,
  WarehouseResponse,
  CreateStockTransactionDto,
  StockTransactionResponse,
  DashboardSummary,
} from './types';

// ===== PRODUCT =====
// [KURAL-6] GET parametreleri camelCase — query string'de convention farklı
export const getProducts = (page = 1, pageSize = 25, search?: string, category?: string) =>
  apiClient.get<PagedResponse<ProductResponse>>(
    `/product/by-company/${COMPANY_ID}`,
    { params: { page, pageSize, search, category } }
  ).then(r => r.data);

export const getProductById = (id: number) =>
  apiClient.get<{ success: boolean; data: ProductResponse }>(
    `/product/${id}`,
    { params: { companyId: COMPANY_ID } }
  ).then(r => r.data.data);

// [KURAL-6] Request Body → PascalCase (case.md rule 6 gereği)
export const createProduct = (dto: Omit<CreateProductDto, 'companyId'>) =>
  apiClient.post<{ success: boolean; message: string; data: ProductResponse }>(
    '/product/create',
    {
      CompanyId: COMPANY_ID,
      ProductName: dto.productName,
      SKU: dto.sku,
      Category: dto.category,
      Unit: dto.unit,
      Description: dto.description,
      MinStockLevel: dto.minStockLevel,
    }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const updateProduct = (dto: Omit<UpdateProductDto, 'companyId'>) =>
  apiClient.post<{ success: boolean; message: string }>(
    '/product/update',
    {
      Id: dto.id,
      CompanyId: COMPANY_ID,
      ProductName: dto.productName,
      SKU: dto.sku,
      Category: dto.category,
      Unit: dto.unit,
      Description: dto.description,
      MinStockLevel: dto.minStockLevel,
    }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const deleteProduct = (id: number) =>
  apiClient.post<{ success: boolean; message: string }>(
    '/product/delete',
    { Id: id, CompanyId: COMPANY_ID }
  ).then(r => r.data);

// ===== WAREHOUSE =====
export const getWarehouses = (page = 1, pageSize = 25, search?: string) =>
  apiClient.get<PagedResponse<WarehouseResponse>>(
    `/warehouse/by-company/${COMPANY_ID}`,
    { params: { page, pageSize, search } }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const createWarehouse = (dto: Omit<CreateWarehouseDto, 'companyId'>) =>
  apiClient.post<{ success: boolean; message: string; data: WarehouseResponse }>(
    '/warehouse/create',
    {
      CompanyId: COMPANY_ID,
      Name: dto.name,
      Location: dto.location,
      Capacity: dto.capacity,
      Description: dto.description,
    }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const updateWarehouse = (dto: Omit<UpdateWarehouseDto, 'companyId'>) =>
  apiClient.post<{ success: boolean; message: string }>(
    '/warehouse/update',
    {
      Id: dto.id,
      CompanyId: COMPANY_ID,
      Name: dto.name,
      Location: dto.location,
      Capacity: dto.capacity,
      Description: dto.description,
    }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const deleteWarehouse = (id: number) =>
  apiClient.post<{ success: boolean; message: string }>(
    '/warehouse/delete',
    { Id: id, CompanyId: COMPANY_ID }
  ).then(r => r.data);

// ===== STOCK TRANSACTION =====
export const getStockTransactions = (
  page = 1,
  pageSize = 25,
  productId?: number,
  warehouseId?: number,
  transactionType?: number
) =>
  apiClient.get<PagedResponse<StockTransactionResponse>>(
    `/stock-transaction/by-company/${COMPANY_ID}`,
    { params: { page, pageSize, productId, warehouseId, transactionType } }
  ).then(r => r.data);

// [KURAL-6] Request Body → PascalCase
export const createStockTransaction = (dto: Omit<CreateStockTransactionDto, 'companyId'>) =>
  apiClient.post<{ success: boolean; message: string; data: StockTransactionResponse }>(
    '/stock-transaction/create',
    {
      CompanyId: COMPANY_ID,
      ProductId: dto.productId,
      WarehouseId: dto.warehouseId,
      TransactionType: dto.transactionType,
      Quantity: dto.quantity,
      Note: dto.note,
    }
  ).then(r => r.data);

// ===== DASHBOARD =====
export const getDashboardSummary = () =>
  apiClient.get<{ success: boolean; data: DashboardSummary }>(
    `/dashboard/summary/${COMPANY_ID}`
  ).then(r => r.data.data);
