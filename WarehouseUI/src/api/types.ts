// ===== TYPES =====

export interface ProductResponse {
  id: number;
  companyId: string;
  productName: string;
  sku: string;
  category: string;
  unit: string;
  description?: string;
  minStockLevel: number;
  currentStock: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductDto {
  companyId: string;
  productName: string;
  sku: string;
  category: string;
  unit: string;
  description?: string;
  minStockLevel: number;
}

export interface UpdateProductDto {
  id: number;
  companyId: string;
  productName: string;
  sku: string;
  category: string;
  unit: string;
  description?: string;
  minStockLevel: number;
}

export interface WarehouseResponse {
  id: number;
  companyId: string;
  name: string;
  location: string;
  capacity: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateWarehouseDto {
  companyId: string;
  name: string;
  location: string;
  capacity: number;
  description?: string;
}

export interface UpdateWarehouseDto {
  id: number;
  companyId: string;
  name: string;
  location: string;
  capacity: number;
  description?: string;
}

export interface StockTransactionResponse {
  id: number;
  companyId: string;
  productId: number;
  productName: string;
  productSKU: string;
  warehouseId: number;
  warehouseName: string;
  transactionType: string;
  quantity: number;
  note?: string;
  createdAt: string;
}

export interface CreateStockTransactionDto {
  companyId: string;
  productId: number;
  warehouseId: number;
  transactionType: number; // 1=In, 2=Out
  quantity: number;
  note?: string;
}

export interface PagedResponse<T> {
  success: boolean;
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardSummary {
  totalProducts: number;
  totalWarehouses: number;
  todayTransactions: number;
  lowStockCount: number;
  totalStockIn: number;
  totalStockOut: number;
  lowStockProducts: LowStockProduct[];
  recentTransactions: RecentTransaction[];
}

export interface LowStockProduct {
  productId: number;
  productName: string;
  sku: string;
  currentStock: number;
  minStockLevel: number;
}

export interface RecentTransaction {
  id: number;
  productName: string;
  warehouseName: string;
  transactionType: string;
  quantity: number;
  createdAt: string;
}
