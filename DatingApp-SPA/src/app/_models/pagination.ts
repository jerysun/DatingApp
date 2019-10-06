export interface Pagination {
    // Corresponding to the Response Headers
    // {"currentPage":2,"itemsPerPage":3,"totalItems":12,"totalPages":4}
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResult<T> {
    result: T;
    pagination: Pagination;
}
