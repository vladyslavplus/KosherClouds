import './App.css'
import { Routes, Route } from 'react-router-dom'
import { Navbar, Footer } from './shared/layouts'
import { LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage } from './pages/auth'
import { ProfilePage } from './pages/profile'
import MenuPage from './pages/menu'
import CartPage from './pages/cart'
import CheckoutPage from './pages/order/CheckoutPage'
import ReviewsPage from './pages/reviews/ReviewsPage'
import PaymentPage from './pages/payment/PaymentPage'
import PaymentSuccessPage from './pages/payment/PaymentSuccessPage'
import ReviewPage from './pages/review/ReviewPage'
import HomePage from './pages/home/HomePage'
import BookingPage from './pages/booking/BookingPage'

function App() {
  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      <Navbar />
      
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path='/menu' element={<MenuPage />} />
        <Route path='/booking' element={<BookingPage />} />
        <Route path='/cart' element={<CartPage />} />
        <Route path='/checkout' element={<CheckoutPage />} />
        <Route path="/review" element={<ReviewPage />} />
        <Route path="/reviews" element={<ReviewsPage />} />
        <Route path="/payment" element={<PaymentPage />} />
        <Route path="/payment-success" element={<PaymentSuccessPage />} />
      </Routes>
      
      <Footer />
    </div>
  )
}

export default App