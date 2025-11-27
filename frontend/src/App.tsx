import './App.css'
import { Routes, Route } from 'react-router-dom'
import { Navbar, Footer, Container } from './shared/layouts'
import { LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage } from './pages/auth'
import { ProfilePage } from './pages/profile'
import MenuPage from './pages/menu'

function HomePage() {
  return (
    <main className="grow py-8">
      <Container>
        <h1 className="text-4xl font-heading font-bold">
          Welcome to Kosher Clouds
        </h1>
      </Container>
    </main>
  );
}

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
        <Route path='menu' element= {<MenuPage />} />
      </Routes>

      <Footer />
    </div>
  )
}

export default App