// Auth types
export interface LoginRequest {
  username: string;
  password: string;
  userType: string;
}

export interface LoginResponse {
  id: number;
  username: string;
  userType: string;
  token: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  userType: string;
}

export interface ChangePasswordRequest {
  username: string;
  oldPassword: string;
  newPassword: string;
}

export interface LoginActivity {
  id: number;
  username: string;
  userType: string;
  loginTime: string;
  ipAddress: string;
}

// Guest types
export interface Guest {
  guestID: string;
  guestName: string;
  address: string;
  city: string;
  contactNo: string;
  idType: string;
  idNumber: string;
  notes: string;
}

export interface CreateGuestRequest {
  guestName: string;
  address: string;
  city: string;
  contactNo: string;
  idType: string;
  idNumber: string;
  notes: string;
}

export type UpdateGuestRequest = CreateGuestRequest;

// Room types
export interface Room {
  roomNo: string;
  roomType: string;
  roomCharges: number;
}

export interface CreateRoomRequest {
  roomNo: string;
  roomType: string;
  roomCharges: number;
}

export interface UpdateRoomRequest {
  roomType: string;
  roomCharges: number;
}

// Reservation types
export interface Reservation {
  id: number;
  guestID: string;
  guestName: string;
  roomNo: string;
  roomType: string;
  roomCharges: number;
  dateIN: string;
  dateOUT: string;
  noOfAdults: number;
  noOfKids: number;
  status: string;
  currency: string;
  notes: string;
}

export interface CreateReservationRequest {
  guestID: string;
  guestName: string;
  roomNo: string;
  roomType: string;
  roomCharges: number;
  dateIN: string;
  dateOUT: string;
  noOfAdults: number;
  noOfKids: number;
  currency: string;
  notes: string;
}

// CheckIn types
export interface CheckInRecord {
  id: number;
  guestID: string;
  roomNo: string;
  roomCharges: number;
  dateIN: string;
  dateOUT: string;
  noOfAdults: number;
  noOfKids: number;
  guestName: string;
  address: string;
  city: string;
  contactNo: string;
  idType: string;
  idNumber: string;
  noOfDays: number;
  totalRoomCharges: number;
  otherCharges: number;
  discountPer: number;
  discount: number;
  subTotal: number;
  serviceTaxPer: number;
  serviceTaxAmount: number;
  luxuryTaxPer: number;
  luxuryTaxAmount: number;
  grandTotal: number;
  totalPaid: number;
  balance: number;
  extraBed: string;
  currency: string;
  status: string;
  notes: string;
}

export interface CreateCheckInRequest {
  guestID: string;
  roomNo: string;
  roomCharges: number;
  dateIN: string;
  dateOUT: string;
  noOfAdults: number;
  noOfKids: number;
  guestName: string;
  address: string;
  city: string;
  contactNo: string;
  idType: string;
  idNumber: string;
  otherCharges: number;
  discountPer: number;
  serviceTaxPer: number;
  luxuryTaxPer: number;
  totalPaid: number;
  extraBed: string;
  currency: string;
  notes: string;
}

export interface TaxCalculationResult {
  noOfDays: number;
  totalRoomCharges: number;
  discount: number;
  subTotal: number;
  serviceTaxAmount: number;
  luxuryTaxAmount: number;
  grandTotal: number;
  balance: number;
}

// CheckOut types
export interface CheckOutRecord {
  id: number;
  billNo: string;
  guestID: string;
  roomNo: string;
  roomCharges: number;
  dateIN: string;
  dateOUT: string;
  noOfDays: number;
  totalRoomCharges: number;
  otherCharges: number;
  discountPer: number;
  discount: number;
  subTotal: number;
  serviceTaxPer: number;
  serviceTaxAmount: number;
  luxuryTaxPer: number;
  luxuryTaxAmount: number;
  educessTax: number;
  educessTaxAmount: number;
  hEducessTax: number;
  hEducessTaxAmount: number;
  grandTotal: number;
  totalPaid: number;
  balance: number;
  extraBed: string;
  currency: string;
  status: string;
  checkOutDate: string;
}

export interface CheckOutRequest {
  checkInId: number;
  totalPaid: number;
  currency: string;
}

// Employee types
export interface Employee {
  employeeID: string;
  employeeName: string;
  address: string;
  mobileNo: string;
  email: string;
  bloodGroup: string;
  gender: string;
  department: string;
  designation: string;
  dateOfJoining: string;
  salary: number;
  basicWorkingTime: string;
}

export interface CreateEmployeeRequest {
  employeeName: string;
  address: string;
  mobileNo: string;
  email: string;
  bloodGroup: string;
  gender: string;
  department: string;
  designation: string;
  dateOfJoining: string;
  salary: number;
  basicWorkingTime: string;
}

export type UpdateEmployeeRequest = CreateEmployeeRequest;

// Attendance types
export interface Attendance {
  id: number;
  employeeID: string;
  employeeName: string;
  workingDate: string;
  status: string;
  overtime: string;
  department: string;
}

export interface AttendanceRequest {
  employeeID: string;
  employeeName: string;
  workingDate: string;
  status: string;
  overtime: string;
  department: string;
}

// Payroll types
export interface ProcessPaymentRequest {
  employeeID: string;
  fromDate: string;
  toDate: string;
  overtimeRate: number;
  deduction: number;
}

export interface PaymentRecord {
  id: number;
  paymentID: string;
  employeeID: string;
  employeeName: string;
  department: string;
  designation: string;
  paymentDate: string;
  fromDate: string;
  toDate: string;
  basicSalary: number;
  presentDays: number;
  salary: number;
  totalOvertime: string;
  overtimeRate: number;
  overtimeAmount: number;
  advance: number;
  deduction: number;
  netPay: number;
}

export interface AdvanceEntryRequest {
  employeeID: string;
  employeeName: string;
  workingDate: string;
  amount: number;
}

export interface AdvanceEntry {
  id: number;
  employeeID: string;
  employeeName: string;
  workingDate: string;
  amount: number;
  deduction: number;
}

// Inventory types
export interface Dish {
  id: number;
  dishName: string;
  category: string;
  rate: number;
}

export interface CreateDishRequest {
  dishName: string;
  category: string;
  rate: number;
}

export interface Beer {
  id: number;
  beerName: string;
  category: string;
  rate: number;
  quantity: number;
}

export interface CreateBeerRequest {
  beerName: string;
  category: string;
  rate: number;
  quantity: number;
}

export interface Liquor {
  id: number;
  liquorName: string;
  category: string;
  rate: number;
  quantity: number;
}

export interface CreateLiquorRequest {
  liquorName: string;
  category: string;
  rate: number;
  quantity: number;
}

export interface LiquorMaster {
  id: number;
  liquorName: string;
  category: string;
  rate: number;
}

export interface CreateLiquorMasterRequest {
  liquorName: string;
  category: string;
  rate: number;
}

export interface Purchase {
  id: number;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  totalAmount: number;
  purchaseDate: string;
  supplier: string;
  notes: string;
}

export interface CreatePurchaseRequest {
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  purchaseDate: string;
  supplier: string;
  notes: string;
}

export interface Stock {
  id: number;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  unit: string;
}

// Order types
export interface Order {
  id: number;
  guestID: string;
  guestName: string;
  roomNo: string;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  totalAmount: number;
  orderDate: string;
  orderType: string;
  notes: string;
}

export interface CreateOrderRequest {
  guestID: string;
  guestName: string;
  roomNo: string;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  notes: string;
}

export interface RestaurantOrder {
  id: number;
  customerName: string;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  totalAmount: number;
  orderDate: string;
  notes: string;
}

export interface CreateRestaurantOrderRequest {
  customerName: string;
  itemName: string;
  category: string;
  quantity: number;
  rate: number;
  notes: string;
}

// Transaction types
export interface Transaction {
  id: number;
  guestID: string;
  guestName: string;
  transactionType: string;
  amount: number;
  transactionDate: string;
  description: string;
  currency: string;
  notes: string;
}

export interface CreateTransactionRequest {
  guestID: string;
  guestName: string;
  transactionType: string;
  amount: number;
  description: string;
  currency: string;
  notes: string;
}

// Hotel Info types
export interface HotelInfo {
  id: number;
  hotelName: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  phone: string;
  email: string;
  website: string;
  tin: string;
  serviceTaxNo: string;
}

export interface HotelInfoRequest {
  hotelName: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  phone: string;
  email: string;
  website: string;
  tin: string;
  serviceTaxNo: string;
}

export interface TaxInfo {
  id: number;
  taxName: string;
  taxPercentage: number;
  description: string;
}

export interface TaxInfoRequest {
  taxName: string;
  taxPercentage: number;
  description: string;
}

export interface ExtraBed {
  id: number;
  bedType: string;
  charges: number;
}

export interface ExtraBedRequest {
  bedType: string;
  charges: number;
}

export interface Currency {
  id: number;
  currencyName: string;
  symbol: string;
}

export interface CurrencyRequest {
  currencyName: string;
  symbol: string;
}

// Dashboard types
export interface DashboardData {
  currentCheckIns: CheckInSummary[];
  currentReservations: ReservationSummary[];
}

export interface CheckInSummary {
  roomNo: string;
  guestID: string;
  guestName: string;
  dateIN: string;
  dateOUT: string;
}

export interface ReservationSummary {
  roomNo: string;
  guestID: string;
  guestName: string;
  dateIN: string;
  dateOUT: string;
}

// Hall & Garden types
export interface Hall {
  id: number;
  hallName: string;
  charges: number;
  description: string;
}

export interface Garden {
  id: number;
  gardenName: string;
  charges: number;
  description: string;
}

export interface HallGardenReservation {
  id: number;
  guestID: string;
  guestName: string;
  hallName: string;
  gardenName: string;
  dateIN: string;
  dateOUT: string;
  noOfDaysHall: number;
  noOfDaysGarden: number;
  rateHall: number;
  rateGarden: number;
  totalHall: number;
  totalGarden: number;
  otherCharges: number;
  discountPer: number;
  discount: number;
  subTotal: number;
  serviceTaxPer: number;
  serviceTaxAmount: number;
  luxuryTaxPer: number;
  luxuryTaxAmount: number;
  grandTotal: number;
  totalPaid: number;
  balance: number;
  currency: string;
  status: string;
  notes: string;
}

export interface CreateHallGardenReservationRequest {
  guestID: string;
  guestName: string;
  hallName: string;
  gardenName: string;
  dateIN: string;
  dateOUT: string;
  noOfDaysHall: number;
  noOfDaysGarden: number;
  rateHall: number;
  rateGarden: number;
  otherCharges: number;
  discountPer: number;
  serviceTaxPer: number;
  luxuryTaxPer: number;
  totalPaid: number;
  currency: string;
  notes: string;
}

export interface HallOrGardenReservation {
  id: number;
  guestID: string;
  guestName: string;
  venueName: string;
  venueType: string;
  dateIN: string;
  dateOUT: string;
  noOfDays: number;
  rate: number;
  totalCharges: number;
  otherCharges: number;
  discountPer: number;
  discount: number;
  subTotal: number;
  serviceTaxPer: number;
  serviceTaxAmount: number;
  luxuryTaxPer: number;
  luxuryTaxAmount: number;
  educessTax: number;
  educessTaxAmount: number;
  hEducessTax: number;
  hEducessTaxAmount: number;
  grandTotal: number;
  totalPaid: number;
  balance: number;
  currency: string;
  status: string;
  notes: string;
}

export interface CreateHallOrGardenReservationRequest {
  guestID: string;
  guestName: string;
  venueName: string;
  venueType: string;
  dateIN: string;
  dateOUT: string;
  noOfDays: number;
  rate: number;
  otherCharges: number;
  discountPer: number;
  serviceTaxPer: number;
  luxuryTaxPer: number;
  totalPaid: number;
  currency: string;
  notes: string;
}

// Schedule types
export interface Schedule {
  id: number;
  subject: string;
  location: string;
  startDate: string;
  endDate: string;
  description: string;
  label: string;
  status: string;
}

export interface ScheduleRequest {
  subject: string;
  location?: string;
  startDate: string;
  endDate: string;
  description?: string;
  label?: string;
  status?: string;
}
