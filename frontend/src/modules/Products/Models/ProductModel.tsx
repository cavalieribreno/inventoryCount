export interface Product {
    productName: string;
    code: string;
    year: number;
    month: number | null;
    totalQuantity: number;
}
export interface ProductDetails{
    id: number,
    code: string,
    year: number,
    month: number | null,
    quantity: number,
    dateHour: string
}