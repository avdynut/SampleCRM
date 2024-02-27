using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SampleCRM.Web.Views
{
    public partial class Customers : BasePage
    {
        private CustomersContext _customersContext => customersDataSource.DomainContext as CustomersContext;
        private OrderContext _orderContext => ordersDataSource.DomainContext as OrderContext;
        private IEnumerable<Models.CountryCodes> CountryCodes => (countryCodesDataSource.DomainContext as CountryCodesContext).CountryCodes;
        private Models.Customers SelectedCustomer => grdCustomers.SelectedItem as Models.Customers;
        private Models.Orders SelectedOrder => grdOrders.SelectedItem as Models.Orders;

        private readonly OrderStatusContext _orderStatusContext = new OrderStatusContext();
        private readonly ShippersContext _shippersContext = new ShippersContext();
        private readonly PaymentTypeContext _paymentTypesContext = new PaymentTypeContext();

        public Customers()
        {
            InitializeComponent();
        }

        private void customersDataSource_LoadedData(object sender, OpenRiaServices.Controls.LoadedDataEventArgs e)
        {
            if (e.HasError)
                return;

            var customers = e.Entities.Cast<Models.Customers>();
            foreach (var customer in customers)
                customer.CountryCodes = CountryCodes;
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);

            if (IsMobileUI)
            {
                grdHead.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

                Grid.SetColumn(grdSearch, 0);
                Grid.SetRow(grdSearch, 2);
                grdSearch.Margin = new Thickness(0, 0, 0, 10);

                Grid.SetRow(customerCard, 0);
                Grid.SetColumn(customerCard, 0);

                Grid.SetColumn(grdCustomerDetails, 0);
                Grid.SetRow(grdCustomerDetails, 1);

                grdTbCustomer.RowDefinitions[0].Height = GridLength.Auto;
                grdTbCustomer.RowDefinitions[1].Height = GridLength.Auto;

                grdTbCustomer.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                grdTbCustomer.ColumnDefinitions[1].Width = GridLength.Auto;

                formCustomer.EditTemplate = Resources["dtNarrowCustomers"] as DataTemplate;


                grdTbOrders.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

                Grid.SetColumn(grdOrderSearch, 0);
                Grid.SetRow(grdOrderSearch, 2);
                grdOrderSearch.Margin = new Thickness(0, 0, 0, 10);
            }
            else
            {
                grdHead.ColumnDefinitions[2].Width = new GridLength(405, GridUnitType.Pixel);

                Grid.SetColumn(grdSearch, 2);
                Grid.SetRow(grdSearch, 0);
                grdSearch.Margin = new Thickness();

                Grid.SetRow(customerCard, 0);
                Grid.SetColumn(customerCard, 0);

                Grid.SetRow(grdCustomerDetails, 0);
                Grid.SetColumn(grdCustomerDetails, 1);

                grdTbCustomer.RowDefinitions[0].Height = new GridLength(2, GridUnitType.Star);
                grdTbCustomer.RowDefinitions[1].Height = GridLength.Auto;

                grdTbCustomer.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                grdTbCustomer.ColumnDefinitions[1].Width = new GridLength(4, GridUnitType.Star);

                formCustomer.EditTemplate = Resources["dtWideCustomers"] as DataTemplate;


                grdTbOrders.ColumnDefinitions[2].Width = new GridLength(405, GridUnitType.Pixel);

                Grid.SetColumn(grdOrderSearch, 2);
                Grid.SetRow(grdOrderSearch, 0);
                grdOrderSearch.Margin = new Thickness();
            }
        }

        #region Search Customer
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCustomer();
        }

        private void btnSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            SearchCustomer();
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchCustomer();
            }
        }

        private void SearchCustomer()
        {
            customersSearchParameter.Value = txtSearch.Text;
            customersDataSource.Load();
        }
        #endregion

        private void formCustomer_EditEnded(object sender, DataFormEditEndedEventArgs e)
        {
            if (e.EditAction == DataFormEditAction.Commit)
            {
                _customersContext.SubmitChanges(OnFormCustomerSubmitCompleted, null);
            }
            else if (e.EditAction == DataFormEditAction.Cancel)
            {
                _customersContext.RejectChanges();
            }
        }

        private void OnFormCustomerSubmitCompleted(SubmitOperation so)
        {
            if (so.HasError)
            {
                if (so.Error.Message.StartsWith("Submit operation failed. Access to operation"))
                {
                    ErrorWindow.Show("Access Denied", "Insuficient User Role", so.Error.Message);
                }
                else
                {
                    ErrorWindow.Show("Access Denied", so.Error.Message, "");
                }

                so.MarkErrorAsHandled();
            }
            else
            {
                //SelectedCustomer.CountryCodes = CountryCodes;
                SelectedCustomer.CountryName = CountryCodes.FirstOrDefault(x => x.CountryCodeID == SelectedCustomer.CountryCode)?.Name;
            }
        }

        #region Search Order
        private void btnOrderSearch_Click(object sender, TappedRoutedEventArgs e)
        {
            SearchOrder();
        }

        private void btnOrderSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            txtOrderSearch.Text = string.Empty;
            SearchOrder();
        }

        private void txtOrderSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchOrder();
            }
        }

        private void SearchOrder()
        {
            ordersSearchParameter.Value = txtOrderSearch.Text;
            ordersDataSource.Load();
        }
        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure to delete the customer and all orders belong that customer?", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                return;

            if (_customersContext.Customers.CanRemove)
            {
                _customersContext.Customers.Remove(SelectedCustomer);
                _customersContext.SubmitChanges(OnDeleteSubmitCompleted, null);
            }
            else
            {
                throw new AccessViolationException("RIA Service Delete Entity for Customer Context is denied");
            }
        }

        private void OnDeleteSubmitCompleted(SubmitOperation so)
        {
            if (so.HasError)
            {
                if (so.Error.Message.StartsWith("Submit operation failed. Access to operation"))
                {
                    ErrorWindow.Show("Access Denied", "Insuficient User Role", so.Error.Message);
                }
                else
                {
                    ErrorWindow.Show("Access Denied", so.Error.Message, "");
                }

                so.MarkErrorAsHandled();
            }
            else
            {
                NavigationService.Refresh();
            }
        }

        private async void btnNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            var result = await CustomerAddEditWindow.Show(new Models.Customers
            {
                IsEditMode = true,
                CountryCodes = CountryCodes,
                CountryCode = CountryCodes.FirstOrDefault().CountryCodeID,
                BirthDateUTC = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }, _customersContext);

            if (result)
            {
                NavigationService.Refresh();
            }
        }

        private async void btnShowOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder == null)
                return;

            SelectedOrder.CountryCodes = CountryCodes;
            SelectedOrder.Customer = SelectedCustomer;
            SelectedOrder.Statuses = (await _orderStatusContext.LoadAsync(_orderStatusContext.GetOrderStatusQuery())).Entities;
            SelectedOrder.Shippers = (await _shippersContext.LoadAsync(_shippersContext.GetShippersQuery())).Entities;
            SelectedOrder.PaymentTypes = (await _paymentTypesContext.LoadAsync(_paymentTypesContext.GetPaymentTypesQuery())).Entities;

            await OrderAddEditWindow.Show(SelectedOrder, _orderContext);
        }

        private async void btnNewOrder_Click(object sender, RoutedEventArgs e)
        {
            await AsyncHelper.RunAsync(ArrangeOrderAddEditWindow);
        }

        private async Task ArrangeOrderAddEditWindow()
        {
            if (SelectedCustomer == null)
                return;

            var order = new Models.Orders
            {
                IsEditMode = true,
                CountryCodes = CountryCodes,
                CustomerID = SelectedCustomer.CustomerID,
                Customer = SelectedCustomer,
                Statuses = (await _orderStatusContext.LoadAsync(_orderStatusContext.GetOrderStatusQuery())).Entities,
                Shippers = (await _shippersContext.LoadAsync(_shippersContext.GetShippersQuery())).Entities,
                PaymentTypes = (await _paymentTypesContext.LoadAsync(_paymentTypesContext.GetPaymentTypesQuery())).Entities
            };

            var result = await OrderAddEditWindow.Show(order, _orderContext);
            if (result)
            {
                NavigationService.Refresh();
            }
        }
    }
}