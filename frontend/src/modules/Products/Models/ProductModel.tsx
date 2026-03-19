export interface GroupedProduct {
    code: string;
    productName: string;
    totalQuantity: number;
}
export interface ProductDetails{
    id: number,
    code: string,
    productName: string,
    year: number,
    month: number | null,
    quantity: number,
    dateHour: string,
    // user information
    userName: string
}